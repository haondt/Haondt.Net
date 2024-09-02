using Haondt.Web.Core.Components;

namespace Haondt.Web.Components
{
    public class IndexModel : IComponentModel
    {
        public required string Title { get; set; }
        public required IComponent Content { get; set; }
        public List<string> StyleSheets { get; set; } = [];
    }
}
