using Haondt.Web.Core.Components;
using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Controllers
{
    [Route("_component")]
    public class ComponentController(IComponentFactory componentFactory) : BaseController
    {
        [Route("{componentIdentity}")]
        public async Task<IActionResult> Get([FromRoute] string componentIdentity)
        {
            var component = await componentFactory.GetComponent(componentIdentity);
            if (component)
                return component.Value.CreateView(this);
            return NotFound();
        }
    }
}
