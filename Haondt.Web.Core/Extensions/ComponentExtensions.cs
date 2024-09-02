using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Core.Extensions
{
    public static class ComponentExtensions
    {
        public static ViewResult CreateView(this IComponent component, Controller controller)
        {
            if (component.ConfigureResponse.HasValue)
            {
                var mutator = new HttpResponseMutator();
                component.ConfigureResponse.Value(mutator);
                mutator.Apply(controller.Response.AsResponseData());
            }

            return controller.View(component.ViewPath, component.Model);
        }
        public static ViewResult CreateView<T>(this IComponent<T> component, Controller controller) where T : IComponentModel
        {
            if (component.ConfigureResponse.HasValue)
            {
                var mutator = new HttpResponseMutator();
                component.ConfigureResponse.Value(mutator);
                mutator.Apply(controller.Response.AsResponseData());
            }

            return controller.View(component.ViewPath, component.Model);
        }
    }
}
