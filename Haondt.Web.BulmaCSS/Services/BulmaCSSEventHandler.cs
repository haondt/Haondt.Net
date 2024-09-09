using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public class BulmaCSSEventHandler(IBulmaCSSLayoutUpdateFactory layoutUpdateFactory) : IEventHandler
    {
        public async Task<Optional<IComponent>> HandleAsync(string eventName, IRequestData requestData)
        {
            if (eventName == "Toast")
            {
                var layoutUpdate = await layoutUpdateFactory
                    .ShowToast(ToastSeverity.Info, "this is  a test message!")
                    .BuildAsync();
                return new(layoutUpdate);
            }

            return new();
        }
    }
}
