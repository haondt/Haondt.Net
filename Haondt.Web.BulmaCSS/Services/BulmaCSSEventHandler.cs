using DotNext;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public class BulmaCSSEventHandler(IBulmaCSSLayoutUpdateFactory layoutUpdateFactory) : IEventHandler
    {
        public async Task<Result<Optional<IComponent>>> HandleAsync(string eventName, IRequestData requestData)
        {
            if (eventName == "Toast")
            {
                var layoutUpdate = await layoutUpdateFactory
                    .ShowToast(ToastSeverity.Info, "this is  a test message!")
                    .BuildAsync();
                if (!layoutUpdate.IsSuccessful)
                    return new(layoutUpdate.Error);
                return new(layoutUpdate);
            }

            return new();
        }
    }
}
