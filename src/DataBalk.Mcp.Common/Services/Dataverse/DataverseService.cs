using DataBalk.Mcp.Common.Model;
using DataBalk.Mcp.Common.Services.HttpConnector;
using DataBalk.Mcp.Common.Services.MicrosoftAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace DataBalk.Mcp.Common.Services.CRM
{

    public class DataverseService : IDataverseService
    {
        private readonly ILogger<DataverseService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpRequestConnector _httpRequestConnector;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMicrosoftAuth _microsoftAuth;
        private readonly string? crmClientId;
        private readonly string? crmClientSecret;
        private readonly string? crmScope;
        private readonly string? crmBaseUrl;
        private readonly string? crmAppId;
        private readonly string? tenantId;


        public DataverseService(
            ILogger<DataverseService> logger,
            IConfiguration configuration,
            IHttpRequestConnector httpRequestConnector,
            IServiceProvider serviceProvider,
            IMicrosoftAuth microsoftAuth

        )
        {
            _logger = logger;
            _httpRequestConnector = httpRequestConnector;
            _configuration = configuration;
            _microsoftAuth = microsoftAuth;
            crmClientId = _configuration["CRM_CLIENT_ID"];
            crmClientSecret = _configuration["CRM_CLIENT_SECRET"];
            crmScope = _configuration["CRM_SCOPE"];
            crmBaseUrl = _configuration["CRM_BASE_URL"];
            crmAppId = _configuration["CRM_KlantDeskAppId"];
            tenantId = _configuration["TenantId"];
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetAccounts(string? query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting accounts from Dataverse with query: {Query}", query);
                var allResults = new List<string>();
                var baseUri = new Uri(crmBaseUrl ?? throw new InvalidOperationException("CRM Base URL is not configured"));
                var currentRequestUri = new Uri(baseUri, $"accounts{query}");

                while (currentRequestUri != null)
                {
                    var result = await _httpRequestConnector.SendHttpGetString<OdataResponse<AccountResponse>>(
                        currentRequestUri,
                        ConnectionEnum.Dataverse,
                        cancellationToken
                    );

                    allResults.Add(result);

                    // Parse the result to get next link
                    // Assuming OdataResponse contains @odata.nextLink property
                    var odataResponse = System.Text.Json.JsonSerializer.Deserialize<OdataResponse<AccountResponse>>(result);
                    currentRequestUri = string.IsNullOrEmpty(odataResponse?.OdataNextLink) ? null : new Uri(odataResponse.OdataNextLink);

                    if (currentRequestUri != null)
                    {
                        _logger.LogInformation($"Fetching next page of accounts from: {odataResponse?.OdataNextLink}", currentRequestUri);
                    }
                }

                var combinedResult = CombineOdataResults(allResults);
                _logger.LogInformation("Successfully retrieved all accounts from Dataverse");
                return combinedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting accounts from Dataverse");
                throw;
            }
        }

        private string CombineOdataResults(List<string> results)
        {
            if (results.Count == 0) return "[]";
            if (results.Count == 1) return results[0];

            var combinedValue = new System.Text.StringBuilder();
            var firstResult = System.Text.Json.JsonDocument.Parse(results[0]);
            var valueProperty = firstResult.RootElement.GetProperty("value");

            combinedValue.Append("{\"value\":[");

            for (int i = 0; i < results.Count; i++)
            {
                var result = System.Text.Json.JsonDocument.Parse(results[i]);
                var values = result.RootElement.GetProperty("value").EnumerateArray();

                foreach (var item in values)
                {
                    combinedValue.Append(item.ToString());
                    combinedValue.Append(',');
                }
            }

            // Remove last comma and close array
            if (combinedValue[combinedValue.Length - 1] == ',')
            {
                combinedValue.Length--;
            }

            combinedValue.Append("]}");
            return combinedValue.ToString();
        }

    }
}
