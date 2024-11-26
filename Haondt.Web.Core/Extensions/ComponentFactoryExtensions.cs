using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Components;

namespace Haondt.Web.Core.Extensions
{
    public static class ComponentFactoryExtensions
    {
        public static Task<IResult> RenderComponentAsync<T>(this IComponentFactory componentFactory, IRequestData? requestData = null, IResponseData? responseData = null) where T : IComponent, new()
        {
            return componentFactory.RenderComponentAsync<T>(new T(), requestData, responseData);
        }
    }
}
