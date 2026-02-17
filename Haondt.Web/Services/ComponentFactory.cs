using Haondt.Web.Components;
using Haondt.Web.Core.Attributes;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Haondt.Web.Services
{
    public class ComponentFactory(
        IHttpContextAccessor httpContext,
        ILayoutComponentFactory layoutFactory) : IComponentFactory
    {
        public Task<IResult> RenderComponentAsync<T>(IRequestData? requestData = null, IResponseData? responseData = null) where T : IComponent, new()
        {
            return RenderComponentAsync<T>(new T(), requestData, responseData);
        }

        public Task<IResult> RenderComponentAsync<T>(T component, IRequestData? requestData = null, IResponseData? responseData = null) where T : IComponent
        {
            return RenderComponentAsync(component, typeof(T), requestData, responseData);
        }

        public Task<IResult> RenderComponentAsync(IComponent component, IRequestData? requestData = null, IResponseData? responseData = null)
        {
            return RenderComponentAsync(component, component.GetType(), requestData, responseData);
        }

        protected virtual IComponent EmbedLayoutIntoPage(IComponent layout)
        {
            return new Page { Content = layout };
        }

        public virtual async Task<IResult> RenderComponentAsync(IComponent component, Type componentType, IRequestData? requestData = null, IResponseData? responseData = null)
        {
            var request = requestData ?? httpContext.HttpContext?.Request.AsRequestData() ?? throw new ArgumentNullException(nameof(requestData));
            var response = responseData ?? httpContext.HttpContext?.Response.AsResponseData() ?? throw new ArgumentNullException(nameof(responseData));

            if (!request.IsHxRequest() && componentType.GetCustomAttributes(typeof(RenderPageAttribute), false).Length != 0)
            {
                component = await layoutFactory.GetLayoutAsync(component);
                component = EmbedLayoutIntoPage(component);
                componentType = component.GetType();
            }


            var rootComponent = new RootComponent
            {
                Component = component,
                Request = request,
                Response = response,
                Type = componentType,
            };

            return new RazorComponentResult<RootComponent>(rootComponent.ToDictionary());
        }
    }
}
