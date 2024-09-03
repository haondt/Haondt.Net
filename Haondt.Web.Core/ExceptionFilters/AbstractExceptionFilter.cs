using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Haondt.Web.Core.ExceptionFilters
{
    public abstract class AbstractExceptionFilter : ExceptionFilterAttribute
    {
        protected readonly Type _exceptionType;
        protected readonly int _code;
        protected readonly Func<ExceptionContext, string>? _messageFactory;

        public AbstractExceptionFilter(Type exceptionType, int code, string message)
        {
            _exceptionType = exceptionType;
            _code = code;
            _messageFactory = _ => message;
        }
        public AbstractExceptionFilter(Type exceptionType, int code)
        {
            _exceptionType = exceptionType;
            _code = code;
        }
        public AbstractExceptionFilter(Type exceptionType, int code, Func<ExceptionContext, string> messageFactory)
        {
            _exceptionType = exceptionType;
            _code = code;
            _messageFactory = messageFactory;
        }

        protected abstract bool CheckExceptionType(Type exceptionType);
        protected virtual IActionResult FormatResult(ExceptionContext context)
        {
            return  new ObjectResult(_messageFactory?.Invoke(context) ?? context.Exception.ToString()) { StatusCode = _code };
        }

        public override void OnException(ExceptionContext context)
        {
            if (CheckExceptionType(context.Exception.GetType()))
                context.Result = FormatResult(context);
            base.OnException(context);
        }
    }

}
