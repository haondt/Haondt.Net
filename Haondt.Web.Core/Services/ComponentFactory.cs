using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Haondt.Web.Core.Services
{
    public class ComponentFactory(IHttpContextAccessor httpContext) : IComponentFactory
    {
        public Task<IResult> RenderComponentAsync<T>(T component, IRequestData? requestData = null, IResponseData? responseData = null) where T : IComponent
        {
            return RenderComponentAsync(component, typeof(T), requestData, responseData);
        }

        public Task<IResult> RenderComponentAsync(IComponent component, IRequestData? requestData = null, IResponseData? responseData = null)
        {
            return RenderComponentAsync(component, component.GetType(), requestData, responseData);
        }

        public Task<IResult> RenderComponentAsync(IComponent component, Type componentType, IRequestData? requestData = null, IResponseData? responseData = null)
        {
            var request = requestData ?? httpContext.HttpContext?.Request.AsRequestData() ?? throw new ArgumentNullException(nameof(requestData));
            var response = responseData ??= httpContext.HttpContext?.Response.AsResponseData() ?? throw new ArgumentNullException(nameof(responseData));

            var rootComponent = new RootComponent
            {
                Component = component,
                Request = request,
                Response = response,
                Type = componentType,
            };

            return Task.FromResult<IResult>(new RazorComponentResult<RootComponent>(rootComponent.ToDictionary()));
        }
    }
}
