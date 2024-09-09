using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Controllers
{
    [Route("_event")]
    public class EventController(IEventPublisher eventPublisher, IHttpContextAccessor httpContext) : BaseController
    {
        [HttpPost("{eventName}")]
        public async Task<IActionResult> Post(string eventName)
        {
            new HxHeaderBuilder()
                .ReSwap("none")
                .Build()(httpContext.HttpContext!.Response.Headers);

            var result = await eventPublisher.PublishAsync(eventName, httpContext.HttpContext!.Request.AsRequestData());
            if (result.HasValue)
                return result.Value.CreateView(this);
            return Ok();
        }
    }
}
