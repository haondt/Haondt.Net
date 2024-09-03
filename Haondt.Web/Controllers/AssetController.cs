using DotNext.Collections.Generic;
using Haondt.Web.Assets;
using Haondt.Web.Core.Controllers;
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
            var (asset, contentType) = result.Value;

            return File(asset, contentType);
        }
    }
}
