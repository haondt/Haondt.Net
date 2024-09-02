using DotNext;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Core.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Haondt.Web.BulmaCSS.Components
{
    public class ToastModel : IComponentModel
    {
        public ToastSeverity Severity { get; set; }
        public Optional<string> Title { get; set; }
        public required string Message { get; set; }
    }
}
