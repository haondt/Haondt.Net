using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;

namespace Haondt.Web.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseHaondtWeb(this WebApplication app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/static",
                ContentTypeProvider = new FileExtensionContentTypeProvider
                {
                    Mappings = { ["._hs"] = "text/hyperscript" }
                }
            });
            return app;
        }

    }
}
