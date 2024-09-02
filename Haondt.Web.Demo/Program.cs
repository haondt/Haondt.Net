using Haondt.Web.Core.Extensions;
using Haondt.Web.Demo;
using Haondt.Web.Demo.Extensions;
using Haondt.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Haondt.Web.Extensions.ServiceCollectionExtensions).Assembly);

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebCoreServices()
    .AddHaondtWebServices(builder.Configuration)
    .AddHaondtWebDemoServices();

builder.Services.AddMvc();

var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

app.Run();
