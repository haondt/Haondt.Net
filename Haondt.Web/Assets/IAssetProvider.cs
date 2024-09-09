using Haondt.Core.Models;
using Haondt.Web.Core.Reasons;

namespace Haondt.Web.Assets
{
    public interface IAssetProvider
    {
        Task<Result<byte[], WebReason>> GetAssetAsync(string path);
    }
}
