using Haondt.Core.Models;
using Haondt.Web.Core.Reasons;
using System.Reflection;

namespace Haondt.Web.Assets
{
    public class ManifestAssetSource(Assembly assembly) : IAssetSource
    {
        private readonly Lazy<HashSet<string>> _paths = new (() => assembly.GetManifestResourceNames().ToHashSet());
        private readonly Assembly _assembly = assembly;

        public async Task<Result<byte[], WebReason>> GetAssetAsync(string assetPath)
        {
            if (!_paths.Value.Contains(assetPath))
                return new (WebReason.NotFound);
            using var stream = _assembly.GetManifestResourceStream(assetPath) 
                ?? throw new FileNotFoundException($"failed to load asset {assetPath}");
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return new(memoryStream.ToArray());
        }
    }
}
