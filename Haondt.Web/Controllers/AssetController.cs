using DotNext.Collections.Generic;
using Haondt.Web.Assets;
using Haondt.Web.Core.Controllers;
using Haondt.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Haondt.Web.Controllers
{
    [Route("_asset")]
    public class AssetController(FileExtensionContentTypeProvider contentTypeProvider, IAssetProvider assetProvider) : BaseController
    {
        private static readonly Dictionary<string, string> _customContentTypes = new()
        {
            { "._hs", "text/hyperscript" }
        };

        [Route("{**assetPath}")]
        public async Task<IActionResult> Get(string assetPath)
        {

            if (assetPath.Contains('/') || assetPath.Contains('\\'))
                return BadRequest("Invalid path.");

            var contentTypeResult = _customContentTypes.TryGetValue(Path.GetExtension(assetPath))
                | contentTypeProvider.TryGetContentType(assetPath);

            if (!contentTypeResult.HasValue)
                return BadRequest("Unsupported file type");

            if (await assetProvider.GetAssetAsync(assetPath) is not { IsSuccessful: true, Value: var asset })
                return NotFound();

            return File(asset, contentTypeResult.Value);
        }
    }
}
