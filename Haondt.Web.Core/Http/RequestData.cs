namespace Haondt.Web.Core.Http
{
    public class RequestData : IRequestData
    {
        private IFormCollection? _form;
        public IFormCollection Form { get => _form ?? throw new NullReferenceException(); set => _form = value; }

        private IQueryCollection? _query;
        public IQueryCollection Query { get => _query ?? throw new NullReferenceException(); set => _query = value; }

        private IRequestCookieCollection? _cookies;
        public IRequestCookieCollection Cookies { get => _cookies ?? throw new NullReferenceException(); set => _cookies = value; }
        private IHeaderDictionary? _headers;
        public IHeaderDictionary Headers { get => _headers ?? throw new NullReferenceException(); set => _headers = value; }
    }
}
