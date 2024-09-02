using Microsoft.Extensions.Primitives;
using System.Collections;

namespace Haondt.Web.Core.Http
{
    public interface IRequestData
    {
        IFormCollection Form { get; }
        IQueryCollection Query { get; }
        IRequestCookieCollection Cookies { get; }
        IHeaderDictionary Headers { get; }
    }
}
