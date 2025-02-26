using Haondt.Core.Models;
using Haondt.Web.Assets;
using Haondt.Web.Core.Reasons;
using Microsoft.AspNetCore.StaticFiles;

namespace Haondt.Web.Services
{
    public class AssetHandler(FileExtensionContentTypeProvider contentTypeProvider, IAssetProvider assetProvider) : IAssetHandler
    {
        private static readonly Dictionary<string, string> _customContentTypes = new()
        {
            { "._hs", "text/hyperscript" }
        };

        public async Task<DetailedResult<(byte[] Content, string ContentType), WebReason>> HandleAsync(string assetPath)
        {
            if (assetPath.Contains('/') || assetPath.Contains('\\'))
                return new(WebReason.NotFound);

            var foundContentType = _customContentTypes.TryGetValue(Path.GetExtension(assetPath), out var customContentType);
            if (!foundContentType)
                foundContentType = contentTypeProvider.TryGetContentType(assetPath, out customContentType);

            if (!foundContentType)
                return new(WebReason.BadRequest);

            if (await assetProvider.GetAssetAsync(assetPath) is not { IsSuccessful: true, Value: var asset })
                return new(WebReason.NotFound);

            return new((asset, customContentType!));
        }
    }
}
