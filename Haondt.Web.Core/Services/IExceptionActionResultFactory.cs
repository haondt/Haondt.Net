using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Core.Services
{
    public interface IExceptionActionResultFactory
    {
        public Task<IActionResult> CreateAsync(Exception exception, HttpContext context);
    }
}
