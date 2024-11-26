using Haondt.Web.BulmaCSS.Components;
using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Demo.Components;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Demo.Controllers
{
    [Route("home")]
    public class HomeController(IComponentFactory componentFactory) : UIController
    {
        public async Task<IResult> GetHome()
        {
            return await componentFactory.RenderComponentAsync<Home>();
        }

        [Route("toast")]
        public async Task<IResult> GetToast()
        {
            return await componentFactory.RenderComponentAsync(new Toast
            {
                Message = "hello from demo!",
                Title = "Greeting"
            });
        }
    }
}

