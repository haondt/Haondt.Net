using Microsoft.AspNetCore.Builder;

namespace Haondt.Web.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseHaondtWeb(this WebApplication app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/static",
            });
            return app;
        }

    }
}
