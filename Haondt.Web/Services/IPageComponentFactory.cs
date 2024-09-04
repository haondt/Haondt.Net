using DotNext;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;

namespace Haondt.Web.Services
{
    public interface IPageComponentFactory
    {
        public Task<Result<IComponent<PageModel>>> GetComponent(string targetComponentName);
        public Task<Result<IComponent<PageModel>>> GetComponent(string targetComponentName, Dictionary<string, string> queryParams);
        public Task<Result<IComponent<PageModel>>> GetComponent(string targetComponentName, List<(string Key, string Value)> queryParams);
        public Task<Result<IComponent<PageModel>>> GetComponent<T>() where T : IComponentModel;
        public Task<Result<IComponent<PageModel>>> GetComponent<T>(Dictionary<string, string> queryParams) where T : IComponentModel;
        public Task<Result<IComponent<PageModel>>> GetComponent<T>(List<(string Key, string Value)> queryParams) where T : IComponentModel;
    }
}
