using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Web.Components.Services
{
    public class NavigationBarSettings
    {
        public string? LogoUri { get; set; }
        public List<NavigationBarEntry> Entries { get; set; } = [];
    }
    public class NavigationBarEntry
    {
        public required string Title { get; set; }
        public required string Url { get; set; }
        public string? PushUrl { get; set; }
    }
}
