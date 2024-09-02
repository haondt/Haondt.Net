using Microsoft.AspNetCore.Mvc.Razor;

namespace Haondt.Web.Demo
{
    public class MyViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return viewLocations.Select(f => f.Replace("/", "/Views/" + context.Values["theme"] + "/"));
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var value = new Random().Next(0, 1);
            var theme = value == 0 ? "Theme1" : "Theme2";
            context.Values["theme"] = theme;
        }
    }
}
