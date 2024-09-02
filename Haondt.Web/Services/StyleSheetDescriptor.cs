using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Web.Services
{
    public class StyleSheetDescriptor : IHeadEntryDescriptor
    {
        public required string Uri { get; init; }
        public string Render() => $"<link rel=\"stylesheet\" href=\"{Uri}\">";
    }
}
