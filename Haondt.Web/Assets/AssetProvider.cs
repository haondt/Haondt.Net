using Haondt.Core.Models;
using Haondt.Web.Core.Reasons;
using Microsoft.Extensions.Caching.Memory;

namespace Haondt.Web.Assets
{
    public class AssetProvider(IEnumerable<IAssetSource> assetSources, IMemoryCache memoryCache) : IAssetProvider
    {
        public async Task<Result<byte[], WebReason>> GetAssetAsync(string assetPath)
        {
            if (memoryCache.TryGetValue<byte[]>(assetPath, out var cachedAsset))
                return new(cachedAsset!);

            foreach (var source in assetSources)
            {
                var result = await source.GetAssetAsync(assetPath);
                if (result.IsSuccessful)
                {
                    memoryCache.Set(assetPath, result.Value, TimeSpan.FromHours(12));
                    return result;
                }
            }

            return new(WebReason.NotFound);
        }
    }
}
