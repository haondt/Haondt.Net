using DotNext;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;

namespace Haondt.Web.Services
{
    public interface IIndexModelComponentFactory
    {
        public Task<Result<IComponent<IndexModel>>> GetComponent(string contentPage);
    }
}
