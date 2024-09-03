namespace Haondt.Web.Services
{
    public class IconDescriptor : IHeadEntryDescriptor
    {
        public required string Uri { get; init; }
        public string Render() => $"<link rel=\"icon\" href=\"{Uri}\">";
    }
}
