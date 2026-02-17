using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Core.Controllers
{
    [Produces("text/html")]
    public class UIController : Controller
    {
        [FromServices]
        public IComponentFactory ComponentFactory { get; set; } = default!;

    }
}
