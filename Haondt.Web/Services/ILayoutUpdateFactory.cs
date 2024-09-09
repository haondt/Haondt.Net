
using Haondt.Web.Core.Components;

namespace Haondt.Web.Services
{
    public interface ILayoutUpdateFactory
    {
        public ILayoutUpdateFactory GetInitialLayout(IComponent content);
        public ILayoutUpdateFactory SetContent(IComponent component);
        public Task<IComponent> BuildAsync();
    }

}
