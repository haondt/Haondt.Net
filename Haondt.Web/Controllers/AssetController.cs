using Haondt.Core.Exceptions;
using Haondt.Web.Core.Reasons;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Controllers
{
    [Route("_asset")]
    public class AssetController(IAssetHandler assetHandler) : Controller
    {
        [Route("{**assetPath}")]
        public async Task<IActionResult> Get(string assetPath)
        {
            var result = await assetHandler.HandleAsync(assetPath);
            if (!result.IsSuccessful)
            {
                return result.Reason switch
                {
                    WebReason.NotFound => NotFound(),
                    WebReason.BadRequest => BadRequest(),
                    WebReason.Unauthorized => Unauthorized(),
                    _ => throw new UnknownReasonException<WebReason>(result.Reason)
                };
            }
            var (asset, contentType) = result.Value;

            return File(asset, contentType);
        }
    }
}
