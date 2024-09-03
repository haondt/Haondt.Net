namespace Haondt.Web.Core.ExceptionFilters
{
    public class ExceptionMessageExceptionFilter : AbstractExceptionFilter
    {
        public ExceptionMessageExceptionFilter(Type exceptionType, int code) : base(exceptionType, code, ctx => ctx.Exception.Message) { }
        protected override bool CheckExceptionType(Type exceptionType) => exceptionType == _exceptionType;
    }
}
