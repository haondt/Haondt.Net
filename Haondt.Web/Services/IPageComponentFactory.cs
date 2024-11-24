namespace Haondt.Web.Services
{
    public interface IPageComponentFactory
    {
        Task<IResult> RenderPageAsync(string path, IReadOnlyDictionary<string, string>? query = null);
    }
}
