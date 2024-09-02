using DotNext;
using Microsoft.AspNetCore.StaticFiles;

namespace Haondt.Web.Extensions
{
    public static class FileExtensionContentTypeProviderExtensions
    {
        public static Optional<string> TryGetContentType(this FileExtensionContentTypeProvider provider, string subpath)
        {
            var result = provider.TryGetContentType(subpath, out var contentType);
            if (result &&  contentType != null)
            {
                return new (contentType);
            }
            return default;
        }
    }
}
