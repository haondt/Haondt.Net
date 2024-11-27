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
                try
                {
                    var result = await _actionResultFactory.CreateAsync(exception, context);
                    await result.ExecuteAsync(context);
                }
                catch (Exception exception2)
                {
                    var message = "Exception occurred:\n\n";
                    message += exception.ToString();
                    message += "\n\nWhile handling that exception, another exception occurred:\n\n";
                    message += exception2.ToString();

                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync(message);
                }
            }
        }
    }
}
