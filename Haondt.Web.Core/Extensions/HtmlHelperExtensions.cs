using Haondt.Web.Core.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Haondt.Web.Core.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static Task<IHtmlContent> PartialAsync(this IHtmlHelper htmlHelper, IComponent component)
        {
            return htmlHelper.PartialAsync(component.ViewPath, component.Model);
        }

        public static Task<IHtmlContent> PartialAsync<T>(this IHtmlHelper htmlHelper, IComponent<T> component) where T : IComponentModel
        {
            return htmlHelper.PartialAsync(component.ViewPath, component.Model);
        }
    }
}
