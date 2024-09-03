namespace Haondt.Web.Core.Http
{
    public interface IResponseData
    {
        IHeaderDictionary Headers { get; }
        int StatusCode { get; set; }
    }
}
