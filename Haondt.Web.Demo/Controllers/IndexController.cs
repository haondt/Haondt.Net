using Haondt.Web.Core.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Demo.Controllers
{
    [Route("/")]
    public class IndexController : BaseController
    {
        public IActionResult Get()
        {
            return Redirect("home");
        }
    }
}
