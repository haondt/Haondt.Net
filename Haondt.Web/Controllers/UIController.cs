using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Core.Controllers
{
    [ServiceFilter(typeof(RenderPageFilter))]
    [Produces("text/html")]
    public class UIController : Controller
    {
    }
}
