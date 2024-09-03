namespace Haondt.Web.Core.ExceptionFilters
{
    public class InheritableExceptionFilter : AbstractExceptionFilter
    {
        public InheritableExceptionFilter(Type exceptionType, int code, string message) : base(exceptionType, code, message) { }
        public InheritableExceptionFilter(Type exceptionType, int code) : base(exceptionType, code) { }
        protected override bool CheckExceptionType(Type exceptionType) => _exceptionType.IsAssignableFrom(exceptionType);
    }
}
