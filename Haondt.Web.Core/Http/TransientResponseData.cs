namespace Haondt.Web.Core.Http
{
    public class TransientResponseData : IResponseData
    {
        private readonly Lazy<IHeaderDictionary> _headersLazy;
        private readonly Action<int> _setStatusCode;
        private readonly Func<int> _getStatusCode;

        public IHeaderDictionary Headers => _headersLazy.Value;

        public int StatusCode
        {
            get => _getStatusCode();
            set => _setStatusCode(value);
        }

        public TransientResponseData(Func<IHeaderDictionary> headersFactory,
            Action<int> setStatusCode,
            Func<int> getStatusCode)
        {
            _headersLazy = new Lazy<IHeaderDictionary>(headersFactory);
            _setStatusCode = setStatusCode;
            _getStatusCode = getStatusCode;
        }
    }
}
