using Haondt.Web.Core.Controllers;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Demo.Controllers
{
    [Route("home")]
    public class HomeController(IPageComponentFactory pageFactory) : BaseController
    {
        public async Task<IResult> GetHome()
        {
            return await pageFactory.RenderPageAsync("/home");
        }
    }
}

