using DotNext;

namespace Haondt.Web.Services
{
    public interface IAssetHandler
    {
        Task<Result<(byte[] Content, string ContentType)>> HandleAsync(string assetPath);
    }
}
