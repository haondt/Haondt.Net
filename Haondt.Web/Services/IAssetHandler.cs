using Haondt.Core.Models;
using Haondt.Web.Core.Reasons;

namespace Haondt.Web.Services
{
    public interface IAssetHandler
    {
        Task<Result<(byte[] Content, string ContentType), WebReason>> HandleAsync(string assetPath);
    }
}
