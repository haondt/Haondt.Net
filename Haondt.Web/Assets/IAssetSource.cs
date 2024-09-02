using DotNext;

namespace Haondt.Web.Assets
{
    public interface IAssetSource
    {
        public Task<Result<byte[]>> GetAssetAsync(string assetPath);
    }
}
