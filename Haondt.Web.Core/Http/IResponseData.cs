namespace Haondt.Web.Core.Http
{
    public interface IResponseData
    {
        IResponseData Status(int statusCode);
        IResponseData Header(string name, string value);
    }
}
