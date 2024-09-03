using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Haondt.Web.Core.ExceptionFilters
{
    public class ExceptionFilter : AbstractExceptionFilter
    {
        public ExceptionFilter(Type exceptionType, int code, string message) : base(exceptionType, code, message) { }
        public ExceptionFilter(Type exceptionType, int code) : base(exceptionType, code) { }
        protected override bool CheckExceptionType(Type exceptionType) => exceptionType == _exceptionType;
    }
}
