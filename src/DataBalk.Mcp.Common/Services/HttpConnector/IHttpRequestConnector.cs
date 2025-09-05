namespace DataBalk.Mcp.Common.Services.HttpConnector
{
    public interface IHttpRequestConnector
    {
        Task<T?> SendHttpGet<T>(
            Uri absoluteUri,
            ConnectionEnum connectionEnum,
            CancellationToken cancellationToken
        );

        Task<string> SendHttpGetString<T>(
            Uri absoluteUri,
            ConnectionEnum connectionEnum,
            CancellationToken cancellationToken
        );
        Task<T> SendHttpPost<T>(
            Uri absoluteUri,
            string payload,
            ConnectionEnum connectionEnum,
            CancellationToken cancellationToken
        );

        public Task<T> SendHttpPostAsync<T>(
            object payload,
            ConnectionEnum connectionEnum,
            string question,
            CancellationToken cancellationToken
        );
    }
}
