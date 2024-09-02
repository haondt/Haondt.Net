using DotNext;
using Haondt.Web.BulmaCSS.Components;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public class BulmaCSSLayoutUpdateFactory(IComponentFactory componentFactory) : IBulmaCSSLayoutUpdateFactory
    {
        public async Task<Result<IComponent>> BuildAsync(List<Func<List<IComponent>, Task<Result<List<IComponent>>>>> buildSteps)
        {
            var components = new List<IComponent>();
            foreach(var buildStep in buildSteps)
            {
                var result = await buildStep(components);
                if (!result.IsSuccessful)
                    return new(result.Error);
                components = result.Value;
            }

            var appendLayout = await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = components
            });
            return appendLayout;
        }

        public Task<Result<IComponent>> BuildAsync()
        {
            return Task.FromResult<Result<IComponent>>(new(new InvalidOperationException($"Builder has not been initialized with any content")));
        }

        private BulmaCSSLayoutUpdateBuilder CreateBuilder() => new(this, componentFactory);

        public ILayoutUpdateFactory GetInitialLayout(IComponent content) => CreateBuilder().GetInitialLayout(content);
        public ILayoutUpdateFactory SetContent(IComponent component) => CreateBuilder().SetContent(component);
        public IBulmaCSSLayoutUpdateFactory ShowToast(ToastSeverity severity, string message) => CreateBuilder().ShowToast(severity, message);
        public IBulmaCSSLayoutUpdateFactory UpdateNavigationBarSelection(string navigationBarSelection) => CreateBuilder().UpdateNavigationBarSelection(navigationBarSelection);
    }
}
