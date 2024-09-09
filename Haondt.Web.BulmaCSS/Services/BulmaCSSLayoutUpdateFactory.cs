using Haondt.Web.BulmaCSS.Components;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public class BulmaCSSLayoutUpdateFactory(IComponentFactory componentFactory) : IBulmaCSSLayoutUpdateFactory
    {
        public async Task<IComponent> BuildAsync(List<Func<List<IComponent>, Task<List<IComponent>>>> buildSteps)
        {
            var components = new List<IComponent>();
            foreach(var buildStep in buildSteps)
                components = await buildStep(components);

            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = components
            });
        }

        public Task<IComponent> BuildAsync()
        {
            throw new InvalidOperationException($"Builder has not been initialized with any content");
        }

        private BulmaCSSLayoutUpdateBuilder CreateBuilder() => new(this, componentFactory);

        public ILayoutUpdateFactory GetInitialLayout(IComponent content) => CreateBuilder().GetInitialLayout(content);
        public ILayoutUpdateFactory SetContent(IComponent component) => CreateBuilder().SetContent(component);
        public IBulmaCSSLayoutUpdateFactory ShowToast(ToastSeverity severity, string message) => CreateBuilder().ShowToast(severity, message);
        public IBulmaCSSLayoutUpdateFactory UpdateNavigationBarSelection(string navigationBarSelection) => CreateBuilder().UpdateNavigationBarSelection(navigationBarSelection);
    }
}
