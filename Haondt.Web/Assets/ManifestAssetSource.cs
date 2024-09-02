using DotNext;
using System.Reflection;

namespace Haondt.Web.Assets
{
    public class ManifestAssetSource(Assembly assembly) : IAssetSource
    {
        private readonly Lazy<HashSet<string>> _paths = new (() => assembly.GetManifestResourceNames().ToHashSet());
        private readonly Assembly _assembly = assembly;

        public async Task<Result<byte[]>> GetAssetAsync(string assetPath)
        {
            if (!_paths.Value.Contains(assetPath))
                return new(new FileNotFoundException(assetPath));
            using var stream = _assembly.GetManifestResourceStream(assetPath);
            if (stream == null)
                return new(new FileNotFoundException(assetPath));
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return new(memoryStream.ToArray());
        }
    }
}
