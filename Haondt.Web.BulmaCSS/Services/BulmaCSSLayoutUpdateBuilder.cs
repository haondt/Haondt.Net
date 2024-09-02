using DotNext;
using Haondt.Web.BulmaCSS.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public class BulmaCSSLayoutUpdateBuilder(BulmaCSSLayoutUpdateFactory parent, IComponentFactory componentFactory) : IBulmaCSSLayoutUpdateFactory
    {
        private List<Func<List<IComponent>, Task<Result<List<IComponent>>>>> _buildSteps = [];

        public Task<Result<IComponent>> BuildAsync()
        {
            return parent.BuildAsync(_buildSteps);
        }

        public ILayoutUpdateFactory GetInitialLayout(IComponent content)
        {
            _buildSteps.Add(async components =>
            {
                var navigationBar = await componentFactory.GetPlainComponent<NavigationBarModel>();
                if (!navigationBar.IsSuccessful)
                    return new(navigationBar.Error);

                var layout = await componentFactory.GetPlainComponent(new Components.DefaultLayoutModel
                {
                    Content = content,
                    NavigationBar = navigationBar.Value
                });
                if (!layout.IsSuccessful)
                    return new(layout.Error);

                components.Add(layout.Value);
                return components;
            });

            return this;
        }

        public ILayoutUpdateFactory SetContent(IComponent component)
        {
            throw new NotImplementedException();
        }

        public IBulmaCSSLayoutUpdateFactory ShowToast(ToastSeverity severity, string message)
        {
            _buildSteps.Add(async components =>
            {
                var toast = await componentFactory.GetPlainComponent(new ToastModel
                {
                    Message = message,
                    Severity = severity
                });
                if (!toast.IsSuccessful)
                    return new(toast.Error);
                components.Add(toast.Value);
                return components;
            });

            return this;
        }

        public IBulmaCSSLayoutUpdateFactory UpdateNavigationBarSelection(string navigationBarSelection)
        {
            throw new NotImplementedException();
        }
    }
}
