using DotNext;

namespace Haondt.Web.Assets
{
    public interface IAssetProvider
    {
        Task<Result<byte[]>> GetAssetAsync(string path);
    }
}
