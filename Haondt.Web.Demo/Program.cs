using Haondt.Web.BulmaCSS.Extensions;
using Haondt.Web.Core.Middleware;
using Haondt.Web.Demo.Extensions;
using Haondt.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
.AddApplicationPart(typeof(Haondt.Web.Extensions.ServiceCollectionExtensions).Assembly);
//.AddApplicationPart(typeof(Haondt.Web.BulmaCSS.Extensions.ServiceCollectionExtensions).Assembly);

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebServices(builder.Configuration)
    .UseBulmaCSS(builder.Configuration)
    .AddHaondtWebDemoServices();

builder.Services.AddMvc();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Run();
