namespace Haondt.Web.Core.Http
{
    public class TransientResponseData : IResponseData
    {
        private readonly Lazy<IHeaderDictionary> _headersLazy;
        private readonly Action<int> _setStatusCode;

        public TransientResponseData(
            Func<IHeaderDictionary> headersFactory,
            Action<int> setStatusCode)
        {
            _headersLazy = new Lazy<IHeaderDictionary>(headersFactory);
            _setStatusCode = setStatusCode;
        }

        public IResponseData Status(int statusCode)
        {
            _setStatusCode(statusCode);
            return this;
        }

        public IResponseData Header(string name, string value)
        {
            _headersLazy.Value[name] = value;
            return this;
        }
    }
}
