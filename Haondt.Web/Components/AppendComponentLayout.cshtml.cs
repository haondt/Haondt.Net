using Haondt.Web.Core.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Haondt.Web.Components
{
    public class AppendComponentLayoutModel : IComponentModel
    {
        public List<IComponent> Components { get; set; } = [];
    }
}
