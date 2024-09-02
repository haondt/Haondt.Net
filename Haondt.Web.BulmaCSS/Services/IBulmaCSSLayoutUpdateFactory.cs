using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public interface IBulmaCSSLayoutUpdateFactory : ILayoutUpdateFactory
    {
        public IBulmaCSSLayoutUpdateFactory UpdateNavigationBarSelection(string navigationBarSelection);
        public IBulmaCSSLayoutUpdateFactory ShowToast(ToastSeverity severity, string message);
    }
}
