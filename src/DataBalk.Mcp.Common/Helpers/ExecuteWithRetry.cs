using Microsoft.Extensions.Logging;

namespace DataBalk.Mcp.Common.Helpers
{
    public class ExecuteWithRetry : IExecuteWithRetry
    {
        private readonly ILogger<ExecuteWithRetry> _logger;

        public ExecuteWithRetry(ILogger<ExecuteWithRetry> logger)
        {
            _logger = logger;
        }

        public async Task<T> Execute<T>(
            Func<Task<T>> operation,
            string operationName,
            int startMilliseconds,
            int maxAttempts,
            CancellationToken cancellationToken
        )
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning($"{ex.Message}");
                    _logger.LogWarning("Retrying......");

                    if (attempt == maxAttempts - 1)
                        throw;

                    int delay = (int)Math.Pow(2, attempt) * startMilliseconds;
                    _logger.LogWarning($"Rate limit hit. Retrying in {delay}ms.");
                    await Task.Delay(delay, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts - 1)
                        throw new Exception(
                            $"Max retry attempts reached for operation '{operationName}': {ex.Message}"
                        );
                    await Task.Delay(TimeSpan.FromSeconds(130), cancellationToken);
                }
            }

            throw new Exception($"Max retry attempts reached for operation '{operationName}'");
        }
    }
}
