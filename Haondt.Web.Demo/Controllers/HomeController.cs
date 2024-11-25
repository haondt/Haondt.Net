using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Services;
using Haondt.Web.Demo.Components;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Demo.Controllers
{
    [Route("home")]
    public class HomeController(IComponentFactory componentFactory) : BaseController
    {
        public async Task<IResult> GetHome()
        {
            //return await pageFactory.RenderPageAsync("/home");
            return await componentFactory.RenderComponentAsync(new Home
            {


            });
        }
    }
}

