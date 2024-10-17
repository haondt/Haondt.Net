using Haondt.Core.Models;
using Haondt.Web.BulmaCSS.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;
using Haondt.Web.Services;

namespace Haondt.Web.BulmaCSS.Services
{
    public class BulmaCSSEventHandler(IComponentFactory componentFactory) : IEventHandler
    {
        public async Task<Optional<IComponent>> HandleAsync(string eventName, IRequestData requestData)
        {
            if (eventName == "Toast")
            {
                var component = await componentFactory.GetPlainComponent<ToastModel>(new ToastModel
                {
                    Message = "this is a test message!",
                    Severity = ToastSeverity.Info,
                });

                return new(component);
            }

            return new();
        }
    }
}
