using DotNext;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;

namespace Haondt.Web.Core.Components
{
    public interface IComponentFactory
    {
        public Task<Result<IComponent>> GetComponent(string componentIdentity,
            IComponentModel? model = null,
            Action<IHttpResponseMutator>? configureResponse = null,
            IRequestData? requestData = null);
        public Task<Result<IComponent<T>>> GetComponent<T>(
            T? model = default,
            Action<IHttpResponseMutator>? configureResponse = null,
            IRequestData? requestData = null) where T : IComponentModel;
        public Task<Result<IComponent>> GetPlainComponent<T>(
            T? model = default,
            Action<IHttpResponseMutator>? configureResponse = null,
            IRequestData? requestData = null) where T : IComponentModel;
    }
}
