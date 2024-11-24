using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Core.Services
{
    public class ExceptionActionResultFactory : IExceptionActionResultFactory
    {
        public Task<IActionResult> CreateAsync(Exception exception, HttpContext context)
        {
            var result = exception switch
            {
                BadHttpRequestException => new ObjectResult(exception.Message) { StatusCode = 400 },
                KeyNotFoundException => new ObjectResult(exception.ToString()) { StatusCode = 404 },
                _ => new ObjectResult(exception.ToString()) { StatusCode = 500 }
            };
            return Task.FromResult<IActionResult>(result);
        }
    }
}
