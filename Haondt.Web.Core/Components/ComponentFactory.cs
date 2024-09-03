using DotNext;
using Haondt.Web.Core.Exceptions;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Components
{
    public class ComponentFactory : IComponentFactory
    {
        private readonly Dictionary<string, IComponentDescriptor> _descriptors;
        private readonly IHttpContextAccessor _httpContext;

        public ComponentFactory(IEnumerable<IComponentDescriptor> descriptors, IHttpContextAccessor httpContext)
        {
            _descriptors = descriptors.ToDictionary(d => d.Identity, d => d);
            _httpContext = httpContext;
        }

        public async Task<Result<IComponent>> GetComponent(string componentIdentity,
            IComponentModel? providedModel = null,
            Action<IHttpResponseMutator>? configureResponse = null,
            IRequestData? requestData = null)
        {
            if (!_descriptors.TryGetValue(componentIdentity, out var descriptor))
                return new(new MissingComponentException(componentIdentity));

            var combinedConfigureResponse = Optional<Action<IHttpResponseMutator>>.None;

            if (descriptor.ConfigureResponse.HasValue || configureResponse != null)
            {
                combinedConfigureResponse = new(m =>
                {
                    if (descriptor.ConfigureResponse.HasValue)
                        descriptor.ConfigureResponse.Value(m);
                    configureResponse?.Invoke(m);
                });
            }

            var model = await GetModel(providedModel, requestData, descriptor);
            if (!model.IsSuccessful)
                return new (model.Error);

            return new Component
            {
                ViewPath = descriptor.ViewPath,
                ConfigureResponse = combinedConfigureResponse,
                Model = model.Value
            };
        }
        public Task<Result<IComponent>> GetPlainComponent<T>(T? providedModel = default, Action<IHttpResponseMutator>? configureResponse = null, IRequestData? requestData = null) where T : IComponentModel
        {
            return GetComponent(ComponentDescriptor<T>.TypeIdentity, providedModel, configureResponse, requestData);
        }

        public async Task<Result<IComponent<T>>> GetComponent<T>(T? providedModel = default, Action<IHttpResponseMutator>? configureResponse = null, IRequestData? requestData = null) where T : IComponentModel
        {
            var componentIdentity = ComponentDescriptor<T>.TypeIdentity;
            if (!_descriptors.TryGetValue(componentIdentity, out var descriptor))
                return new(new MissingComponentException(typeof(T).Name));

            if (descriptor is not ComponentDescriptor<T> typedDescriptor)
                return new (new InvalidCastException($"Component descriptor has an identity of {componentIdentity}, was expecting type {typeof(ComponentDescriptor<T>)} but found {descriptor.GetType()}"));

            var combinedConfigureResponse = Optional<Action<IHttpResponseMutator>>.None;

            if (descriptor.ConfigureResponse.HasValue || configureResponse != null)
            {
                combinedConfigureResponse = new(m =>
                {
                    if (descriptor.ConfigureResponse.HasValue)
                        descriptor.ConfigureResponse.Value(m);
                    configureResponse?.Invoke(m);
                });
            }

            var model = await GetModel<T>(providedModel, requestData, typedDescriptor);
            if (!model.IsSuccessful)
                return new (model.Error);

            return new Component<T>
            {
                ViewPath = descriptor.ViewPath,
                ConfigureResponse = combinedConfigureResponse,
                Model = model.Value
            };

        }

        private async Task<Result<IComponentModel>> GetModel(IComponentModel? providedModel, IRequestData? providedRequestData,IComponentDescriptor descriptor)
        {
            if (providedModel != null)
                return new (providedModel);

            if (descriptor.DefaultModelFactory.HasValue)
            {
                if (providedRequestData != null)
                    return await descriptor.DefaultModelFactory.Value(this, providedRequestData);
                else if (_httpContext.HttpContext != null)
                    return await descriptor.DefaultModelFactory.Value(this, _httpContext.HttpContext.Request.AsRequestData());
            }

            if (descriptor.DefaultNoRequestDataModelFactory.HasValue)
                return await descriptor.DefaultNoRequestDataModelFactory.Value(this);

            return new(new InvalidOperationException($"Unable to render component {descriptor.Identity}"));
        }

        private async Task<Result<T>> GetModel<T>(T? providedModel, IRequestData? providedRequestData, IComponentDescriptor<T> descriptor) where T : IComponentModel
        {
            if (providedModel != null)
                return new (providedModel);

            if (descriptor.DefaultModelFactory.HasValue)
            {
                if (providedRequestData != null)
                    return await descriptor.DefaultModelFactory.Value(this, providedRequestData);
                else if (_httpContext.HttpContext != null)
                    return await descriptor.DefaultModelFactory.Value(this, _httpContext.HttpContext.Request.AsRequestData());
            }

            if (descriptor.DefaultNoRequestDataModelFactory.HasValue)
                return await descriptor.DefaultNoRequestDataModelFactory.Value(this);

            return new(new InvalidOperationException($"Unable to render component {descriptor.Identity}"));
        }
    }

}
