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

            var responseConfigurationSteps = new List<Action<IHttpResponseMutator>>();
            if (descriptor.ConfigureResponse.HasValue)
                responseConfigurationSteps.Add(descriptor.ConfigureResponse.Value);
            if (configureResponse != null)
                responseConfigurationSteps.Add(configureResponse);

            var (model, additionalConfiguration) = await GetModel(providedModel, requestData, descriptor);
            if (additionalConfiguration.HasValue)
                responseConfigurationSteps.Add(additionalConfiguration.Value);

            return new Component
            {
                ViewPath = descriptor.ViewPath,
                ConfigureResponse = responseConfigurationSteps.Count == 0 ? new() : new(m =>
                {
                    foreach (var action in responseConfigurationSteps)
                        action.Invoke(m);
                }),
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

            var responseConfigurationSteps = new List<Action<IHttpResponseMutator>>();
            if (descriptor.ConfigureResponse.HasValue)
                responseConfigurationSteps.Add(descriptor.ConfigureResponse.Value);
            if (configureResponse != null)
                responseConfigurationSteps.Add(configureResponse);

            var (model, additionalConfiguration) = await GetModel<T>(providedModel, requestData, typedDescriptor);
            if (additionalConfiguration.HasValue)
                responseConfigurationSteps.Add(additionalConfiguration.Value);

            return new Component<T>
            {
                ViewPath = descriptor.ViewPath,
                ConfigureResponse = responseConfigurationSteps.Count == 0 ? new() : new(m =>
                {
                    foreach (var action in responseConfigurationSteps)
                        action.Invoke(m);
                }),
                Model = model
            };
        }

        private async Task<(IComponentModel Model, Optional<Action<IHttpResponseMutator>> AdditionalConfiguration)> GetModel(IComponentModel? providedModel, IRequestData? providedRequestData, IComponentDescriptor descriptor)
        {
            if (providedModel != null)
                return (providedModel, new());

            if (descriptor.DefaultModelFactory.HasValue)
            {
                if (providedRequestData != null)
                    return await descriptor.DefaultModelFactory.Value(this, providedRequestData);
                else if (httpContext.HttpContext != null)
                    return await descriptor.DefaultModelFactory.Value(this, httpContext.HttpContext.Request.AsRequestData());
            }

            if (descriptor.DefaultNoRequestDataModelFactory.HasValue)
                return (await descriptor.DefaultNoRequestDataModelFactory.Value(this), new());

            throw new InvalidOperationException($"Unable to render component {descriptor.Identity}");
        }

        private async Task<(T Model, Optional<Action<IHttpResponseMutator>> AdditionalConfiguration)> GetModel<T>(T? providedModel, IRequestData? providedRequestData, IComponentDescriptor<T> descriptor) where T : IComponentModel
        {
            if (providedModel != null)
                return (providedModel, new());

            if (descriptor.DefaultModelFactory.HasValue)
            {
                if (providedRequestData != null)
                    return await descriptor.DefaultModelFactory.Value(this, providedRequestData);
                else if (httpContext.HttpContext != null)
                    return await descriptor.DefaultModelFactory.Value(this, httpContext.HttpContext.Request.AsRequestData());
            }

            if (descriptor.DefaultNoRequestDataModelFactory.HasValue)
                return (await descriptor.DefaultNoRequestDataModelFactory.Value(this), new());

            throw new InvalidOperationException($"Unable to render component {descriptor.Identity}");
        }
    }

}
