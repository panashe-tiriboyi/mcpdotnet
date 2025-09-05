namespace DataBalk.Mcp.Common.Services.MicrosoftAuth
{
    public interface IMicrosoftAuth
    {
        Task<string> GetAccessToken(ConnectionEnum connection, CancellationToken cancellation);
    }
}
