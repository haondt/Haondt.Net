using Haondt.Core.Models;
using Haondt.Web.Core.Reasons;

namespace Haondt.Web.Assets
{
    public interface IAssetProvider
    {
        Task<DetailedResult<byte[], WebReason>> GetAssetAsync(string path);
    }
}
