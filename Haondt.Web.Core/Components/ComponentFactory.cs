using Haondt.Core.Models;
using Haondt.Web.Core.Exceptions;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Components
{
    public class ComponentFactory(IEnumerable<IComponentDescriptor> descriptors, IHttpContextAccessor httpContext) : IComponentFactory
    {
        private readonly Dictionary<string, IComponentDescriptor> _descriptors = descriptors.ToDictionary(d => d.Identity, d => d);

        public async Task<IComponent> GetComponent(string componentIdentity,
            IComponentModel? providedModel = null,
            Action<IHttpResponseMutator>? configureResponse = null,
            IRequestData? requestData = null)
        {
            if (!_descriptors.TryGetValue(componentIdentity, out var descriptor))
                throw new MissingComponentException(componentIdentity);

            var combinedConfigureResponse = new Optional<Action<IHttpResponseMutator>>();

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

            return new Component
            {
                ViewPath = descriptor.ViewPath,
                ConfigureResponse = combinedConfigureResponse,
                Model = model
            };
        }
        public Task<IComponent> GetPlainComponent<T>(T? providedModel = default, Action<IHttpResponseMutator>? configureResponse = null, IRequestData? requestData = null) where T : IComponentModel
        {
            return GetComponent(ComponentDescriptor<T>.TypeIdentity, providedModel, configureResponse, requestData);
        }

        public async Task<IComponent<T>> GetComponent<T>(T? providedModel = default, Action<IHttpResponseMutator>? configureResponse = null, IRequestData? requestData = null) where T : IComponentModel
        {
            var componentIdentity = ComponentDescriptor<T>.TypeIdentity;
            if (!_descriptors.TryGetValue(componentIdentity, out var descriptor))
                throw new MissingComponentException(typeof(T).Name);

            if (descriptor is not ComponentDescriptor<T> typedDescriptor)
                throw new InvalidCastException($"Component descriptor has an identity of {componentIdentity}, was expecting type {typeof(ComponentDescriptor<T>)} but found {descriptor.GetType()}");

            var combinedConfigureResponse = new Optional<Action<IHttpResponseMutator>>();

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

            return new Component<T>
            {
                ViewPath = descriptor.ViewPath,
                ConfigureResponse = combinedConfigureResponse,
                Model = model
            };

        }

        private async Task<IComponentModel> GetModel(IComponentModel? providedModel, IRequestData? providedRequestData,IComponentDescriptor descriptor)
        {
            if (providedModel != null)
                return providedModel;

            if (descriptor.DefaultModelFactory.HasValue)
            {
                if (providedRequestData != null)
                    return await descriptor.DefaultModelFactory.Value(this, providedRequestData);
                else if (httpContext.HttpContext != null)
                    return await descriptor.DefaultModelFactory.Value(this, httpContext.HttpContext.Request.AsRequestData());
            }

            if (descriptor.DefaultNoRequestDataModelFactory.HasValue)
                return await descriptor.DefaultNoRequestDataModelFactory.Value(this);

            throw new InvalidOperationException($"Unable to render component {descriptor.Identity}");
        }

        private async Task<T> GetModel<T>(T? providedModel, IRequestData? providedRequestData, IComponentDescriptor<T> descriptor) where T : IComponentModel
        {
            if (providedModel != null)
                return providedModel;

            if (descriptor.DefaultModelFactory.HasValue)
            {
                if (providedRequestData != null)
                    return await descriptor.DefaultModelFactory.Value(this, providedRequestData);
                else if (httpContext.HttpContext != null)
                    return await descriptor.DefaultModelFactory.Value(this, httpContext.HttpContext.Request.AsRequestData());
            }

            if (descriptor.DefaultNoRequestDataModelFactory.HasValue)
                return await descriptor.DefaultNoRequestDataModelFactory.Value(this);

            throw new InvalidOperationException($"Unable to render component {descriptor.Identity}");
        }
    }

}
