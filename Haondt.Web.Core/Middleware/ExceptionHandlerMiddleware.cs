using Haondt.Web.Core.Services;

namespace Haondt.Web.Core.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IExceptionActionResultFactory _actionResultFactory;

        public ExceptionHandlerMiddleware(RequestDelegate next,
            IExceptionActionResultFactory actionResultFactory)
        {
            _next = next;
            _actionResultFactory = actionResultFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var actionResult = await _actionResultFactory.CreateAsync(ex, context);
                await actionResult.ExecuteResultAsync(new Microsoft.AspNetCore.Mvc.ActionContext
                {
                    HttpContext = context,
                    RouteData = context.GetRouteData(),
                    ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
                });
            }
        }
    }
}
