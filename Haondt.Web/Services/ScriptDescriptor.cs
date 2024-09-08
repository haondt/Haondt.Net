namespace Haondt.Web.Services
{
    public class ScriptDescriptor : IHeadEntryDescriptor
    {
        public required string Uri { get; init; }
        public string? CrossOrigin { get; init; }
        public string Render() => CrossOrigin != null
            ? $"<script src=\"{Uri}\" crossorigin=\"{CrossOrigin}\"></script>"
            : $"<script src=\"{Uri}\"></script>";
    }
}
