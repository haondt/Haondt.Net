using Haondt.Web.Core.Http;
using Newtonsoft.Json;

namespace Haondt.Web.Core.Services
{
    public class HxHeaderBuilder
    {
        private const string HX_PUSH_URL = "HX-Push-Url";
        private const string HX_TRIGGER_AFTER_SETTLE = "HX-Trigger-After-Settle";
        private const string HX_RESWAP = "HX-Reswap";
        private const string HX_RETARGET = "HX-Retarget";
        private const string HX_RESELECT = "HX-Reselect";

        private string? _pushUrl;
        private string? _reSwap;
        private string? _retarget;
        private string? _reselect;
        private Dictionary<string, object>? _triggerAfterSettle;

        public HxHeaderBuilder PushUrl(string url)
        {
            if (_pushUrl != null)
                throw new InvalidOperationException("Push url already set");
            _pushUrl = url;
            return this;
        }

        public HxHeaderBuilder TriggerAfterSettle(string @event, object payload)
        {
            _triggerAfterSettle ??= [];
            if (_triggerAfterSettle.ContainsKey(@event))
                throw new InvalidOperationException($"{@event} already configured in trigger after settle");
            _triggerAfterSettle[@event] = payload;

            return this;
        }

        public HxHeaderBuilder ReSwap(string method)
        {
            if (_reSwap != null)
                throw new InvalidOperationException("Swap method already set");
            _reSwap = method;
            return this;
        }

        public HxHeaderBuilder ReTarget(string target)
        {
            if (_retarget != null)
                throw new InvalidOperationException("Target already set");
            _retarget = target;
            return this;
        }
        public HxHeaderBuilder ReSelect(string selector)
        {
            if (_reselect != null)
                throw new InvalidOperationException("Selector already set");
            _reselect = selector;
            return this;
        }

        public Action<IHeaderDictionary> Build()
        {
            List<Action<IHeaderDictionary>> actions = [];
            if (_pushUrl != null)
                actions.Add(h => h[HX_PUSH_URL] = _pushUrl);
            if (_reSwap != null)
                actions.Add(h => h[HX_RESWAP] = _reSwap);
            if (_retarget != null)
                actions.Add(h => h[HX_RETARGET] = _retarget);
            if (_reselect != null)
                actions.Add(h => h[HX_RESELECT] = _reselect);
            if (_triggerAfterSettle != null && _triggerAfterSettle.Count > 0)
            {
                var payload = JsonConvert.SerializeObject(_triggerAfterSettle);
                actions.Add(h => h[HX_TRIGGER_AFTER_SETTLE] = payload);
            }

            return (h) =>
            {
                foreach (var action in actions)
                    action.Invoke(h);
            };
        }

    }

    public static class HxHeaderBuilderExtensions
    {
        public static Action<IHttpResponseMutator> BuildResponseMutator(this HxHeaderBuilder builder)
        {
            var headerDictionaryMutator = builder.Build();
            return (m => m.ConfigureHeadersAction = headerDictionaryMutator);
        }
    }
}

