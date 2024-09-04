using System.Web;

namespace Haondt.Web.Services
{
    public class MetaDescriptor : IHeadEntryDescriptor
    {
        public required string Name { get; set; }
        public required string Content { get; set; }
        public string Render() => $"<meta name=\"{Name}\" content=\"{HttpUtility.HtmlEncode(Content)}\" />";
    }
}
