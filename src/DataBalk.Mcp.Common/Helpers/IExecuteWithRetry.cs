namespace DataBalk.Mcp.Common.Helpers
{
    public interface IExecuteWithRetry
    {
        Task<T> Execute<T>(
            Func<Task<T>> operation,
            string operationName,
            int startMilliseconds,
            int maxAttempts,
            CancellationToken cancellationToken
        );
    }
}

