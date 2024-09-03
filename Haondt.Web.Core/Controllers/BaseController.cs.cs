using Haondt.Web.Core.ExceptionFilters;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Core.Controllers
{
    [ExceptionFilter(typeof(KeyNotFoundException), 404)]
    [ExceptionMessageExceptionFilter(typeof(BadHttpRequestException), 400)]
    [InheritableExceptionFilter(typeof(Exception), 500)]
    [Produces("text/html")]
    public class BaseController : Controller
    {
    }
}
