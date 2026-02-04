using Haondt.Web.Core.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Haondt.Web.Services
{
    public interface IComponentFactory
    {
        public Task<IResult> RenderComponentAsync<T>(IRequestData? requestData = null, IResponseData? responseData = null) where T : IComponent, new();
        public Task<IResult> RenderComponentAsync<T>(T component, IRequestData? requestData = null, IResponseData? responseData = null) where T : IComponent;
        public Task<IResult> RenderComponentAsync(IComponent component, IRequestData? requestData = null, IResponseData? responseData = null);
        public Task<IResult> RenderComponentAsync(IComponent component, Type componentType, IRequestData? requestData = null, IResponseData? responseData = null);
    }
}
