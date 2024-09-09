using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Web.BulmaCSS.Components
{
    public class NavigationBarModel : IComponentModel
    {
        public Optional<string> LogoUri { get; set; }
        public Optional<string> LogoClickUri{ get; set; }
        public List<NavigationBarEntry> NavigationBarEntries { get; set; } = [];
    }

    public class NavigationBarEntry
    {
        public required string Title { get; set; }
        public required string Url { get; set; }
        public Optional<string> PushUrl { get; set; } = new Optional<string>();
    }
}
