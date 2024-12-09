namespace Haondt.Web.Core.Services
{
    public interface IExceptionActionResultFactory
    {
        public Task<IResult> CreateAsync(Exception exception, HttpContext context);
    }
}
