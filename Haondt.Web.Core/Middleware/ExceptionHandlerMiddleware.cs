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
            catch (Exception exception)
            {
                var (message, status) = exception switch
                {
                    BadHttpRequestException => (exception.Message, 400),
                    KeyNotFoundException => (exception.ToString(), 404),
                    _ => (exception.ToString(), 500)
                };
                context.Response.StatusCode = status;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(message);
            }
        }
    }
}
