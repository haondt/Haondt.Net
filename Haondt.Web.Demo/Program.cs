using Haondt.Web.BulmaCSS.Extensions;
using Haondt.Web.Core.Middleware;
using Haondt.Web.Demo.UI.Extensions;
using Haondt.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebServices(builder.Configuration)
    .UseBulmaCSS(builder.Configuration)
    .AddHaondtWebDemoUI();

builder.Services.AddMvc();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHaondtWeb();

app.Run();
