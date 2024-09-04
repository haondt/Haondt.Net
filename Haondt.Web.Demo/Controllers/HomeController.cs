using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Exceptions;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Demo.Components;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Haondt.Web.Demo.Controllers
{
    [Route("home")]
    public class HomeController(IPageComponentFactory pageFactory) : BaseController
    {

        public async Task<IActionResult> Get()
        {
            var indexComponent = await pageFactory.GetComponent<HomeModel>();
            return indexComponent.Value.CreateView(this);
        }
    }
}

