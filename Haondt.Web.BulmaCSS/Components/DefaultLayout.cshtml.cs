using Haondt.Web.Core.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Haondt.Web.BulmaCSS.Components
{
    public class DefaultLayoutModel : IComponentModel
    {
        public required IComponent Content { get; set; }
        public required IComponent NavigationBar { get; set; }
    }
}
