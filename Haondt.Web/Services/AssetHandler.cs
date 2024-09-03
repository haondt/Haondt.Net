using DotNext;
using DotNext.Collections.Generic;
using Haondt.Web.Assets;
using Haondt.Web.Extensions;
using Microsoft.AspNetCore.StaticFiles;

namespace Haondt.Web.Services
{
    public class AssetHandler(FileExtensionContentTypeProvider contentTypeProvider, IAssetProvider assetProvider) : IAssetHandler
    {
        private static readonly Dictionary<string, string> _customContentTypes = new()
        {
            { "._hs", "text/hyperscript" }
        };

        public async Task<Result<(byte[] Content, string ContentType)>> HandleAsync(string assetPath)
        {
            if (assetPath.Contains('/') || assetPath.Contains('\\'))
                return new(new BadHttpRequestException("Invalid path."));

            var contentTypeResult = _customContentTypes.TryGetValue(Path.GetExtension(assetPath))
                | contentTypeProvider.TryGetContentType(assetPath);

            if (!contentTypeResult.HasValue)
                return new(new BadHttpRequestException("Unsupported file type"));

            if (await assetProvider.GetAssetAsync(assetPath) is not { IsSuccessful: true, Value: var asset })
                return new (new KeyNotFoundException(assetPath));

            return new((asset, contentTypeResult.Value));
        }
    }
}
