namespace Haondt.Web.Core.Http
{
    public class TransientResponseData : IResponseData
    {
        private readonly Lazy<IHeaderDictionary> _headersLazy;
        public IHeaderDictionary Headers => _headersLazy.Value;

        public TransientResponseData(Func<IHeaderDictionary> headersFactory)
        {
            _headersLazy = new Lazy<IHeaderDictionary>(headersFactory);
        }
    }
}
