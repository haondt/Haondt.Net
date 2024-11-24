using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Services
{
    public interface IComponentFactory
    {
        public Task<IResult> RenderComponentAsync<T>(T component, IRequestData? requestData = null, IResponseData? responseData = null) where T : IComponent;
        public Task<IResult> RenderComponentAsync(IComponent component, IRequestData? requestData = null, IResponseData? responseData = null);
        public Task<IResult> RenderComponentAsync(IComponent component, Type componentType, IRequestData? requestData = null, IResponseData? responseData = null);
    }
}
