using Haondt.Core.Exceptions;
using Haondt.Web.Assets;
using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Reasons;
using Haondt.Web.Extensions;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Haondt.Web.Controllers
{
    [Route("_asset")]
    public class AssetController(IAssetHandler assetHandler) : BaseController
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
