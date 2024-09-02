namespace Haondt.Web.Core.Http
{
    public class TransientRequestData : IRequestData
    {
        private readonly Lazy<IFormCollection> _formLazy;
        public IFormCollection Form => _formLazy.Value;
        private readonly Lazy<IQueryCollection> _queryLazy;
        public IQueryCollection Query => _queryLazy.Value;
        private readonly Lazy<IRequestCookieCollection> _cookiesLazy;
        public IRequestCookieCollection Cookies => _cookiesLazy.Value;
        private readonly Lazy<IHeaderDictionary> _headersLazy;
        public IHeaderDictionary Headers => _headersLazy.Value;

        public TransientRequestData(
            Func<IFormCollection> formFactory,
            Func<IQueryCollection> queryFactory,
            Func<IRequestCookieCollection> cookiesFactory,
            Func<IHeaderDictionary> headersFactory)
        {
            _formLazy = new Lazy<IFormCollection>(formFactory);
            _queryLazy = new Lazy<IQueryCollection>(queryFactory);
            _cookiesLazy = new Lazy<IRequestCookieCollection>(cookiesFactory);
            _headersLazy = new Lazy<IHeaderDictionary>(headersFactory);
        }
    }
}
