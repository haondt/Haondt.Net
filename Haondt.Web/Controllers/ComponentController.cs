using Haondt.Web.Core.Components;
using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Controllers
{
    [Route("_component")]
    public class ComponentController(IComponentHandler componentHandler) : BaseController
    {
        [Route("{componentIdentity}")]
        public async Task<IActionResult> Get([FromRoute] string componentIdentity)
        {
            var component = await componentHandler.HandleAsync(componentIdentity);
            return component.CreateView(this);
        }
    }
}
