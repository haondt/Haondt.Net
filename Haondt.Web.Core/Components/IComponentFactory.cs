using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;

namespace Haondt.Web.Core.Components
{
    public interface IComponentFactory
    {
        public Task<IComponent> GetComponent(string componentIdentity,
            IComponentModel? model = null,
            Action<IHttpResponseMutator>? configureResponse = null,
            IRequestData? requestData = null);
        public Task<IComponent<T>> GetComponent<T>(
            T? model = default,
            Action<IHttpResponseMutator>? configureResponse = null,
            IRequestData? requestData = null) where T : IComponentModel;
        public Task<IComponent> GetPlainComponent<T>(
            T? model = default,
            Action<IHttpResponseMutator>? configureResponse = null,
            IRequestData? requestData = null) where T : IComponentModel;
    }
}
