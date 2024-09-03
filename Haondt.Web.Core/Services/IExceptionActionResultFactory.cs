using DotNext;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Core.Services
{
    public interface IExceptionActionResultFactory
    {
        public Task<Result<IActionResult>> CreateAsync(Exception exception, HttpContext context);
    }
}
