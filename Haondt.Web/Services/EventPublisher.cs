using DotNext;
using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Services
{
    public class EventPublisher(IEnumerable<IEventHandler> handlers, IComponentFactory componentFactory) : IEventPublisher
    {
        public async Task<Result<Optional<IComponent>>> PublishAsync(string eventName, IRequestData requestData)
        {
            var components = new List<IComponent>();
            foreach(var handler in handlers)
            {
                var result = await handler.HandleAsync(eventName, requestData);
                if (!result.IsSuccessful)
                    return new(result.Error);
                if (result.Value.HasValue)
                    components.Add(result.Value.Value);
            }

            if (components.Count == 0)
                return new();
            if (components.Count == 1)
                return new(new Optional<IComponent>(components[0]));
            var appendLayout = await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
            {
                Components = components
            });
            return new(appendLayout);
        }
    }
}
