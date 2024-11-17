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
                if (!result.IsSuccessful)
                    continue;
                var (data, cache) = result.Value;
                if (cache)
                    memoryCache.Set(assetPath, data, TimeSpan.FromHours(12));
                return new(data);
            }

            return new(WebReason.NotFound);
        }
    }
}
