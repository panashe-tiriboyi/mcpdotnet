using DataBalk.Mcp.Common.Helpers;
using DataBalk.Mcp.Common.Services.MicrosoftAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DataBalk.Mcp.Common.Services.HttpConnector
{
    public class HttpRequestConnector : IHttpRequestConnector
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IMicrosoftAuth _microsoftAuth;
        private readonly ILogger<HttpRequestConnector> _logger;
        private readonly string? API_KEY;
        private readonly string? ENDPOINT;
        private readonly IExecuteWithRetry _executeWithRetry;

        public HttpRequestConnector(
            IHttpClientFactory clientFactory,
            ILogger<HttpRequestConnector> logger,
            IMicrosoftAuth microsoftAuth,
            IConfiguration configuration,
            IExecuteWithRetry executeWithRetry
        )
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
            _microsoftAuth = microsoftAuth;
            _logger = logger;
            API_KEY = _configuration["MODEL:API_KEY"];
            ENDPOINT = _configuration["MODEL:ENDPOINT"];
            _executeWithRetry = executeWithRetry;
        }

        public async Task<T?> SendHttpGet<T>(
            Uri relativeUri,
            ConnectionEnum connectionEnum,
            CancellationToken cancellationToken
        )
        {
            var httpClient = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
            string accessToken = await _microsoftAuth.GetAccessToken(
                connectionEnum,
                cancellationToken
            );

            switch (connectionEnum)
            {
                case ConnectionEnum.Graph:
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            accessToken
                        );
                    break;
                case ConnectionEnum.Dataverse:
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            accessToken
                        );
                    break;
                case ConnectionEnum.OpenAI:
                    break;
                default:
                    throw new InvalidDataException("Connection not supported");
            }

            var response = await httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseObject = JsonSerializer.Deserialize<T>(responseContent);

            return responseObject;
        }

        public async Task<string> SendHttpGetString<T>(
            Uri relativeUri,
            ConnectionEnum connectionEnum,
            CancellationToken cancellationToken
        )
        {
            var httpClient = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
            string accessToken = await _microsoftAuth.GetAccessToken(
                connectionEnum,
                cancellationToken
            );

            switch (connectionEnum)
            {
                case ConnectionEnum.Graph:
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            accessToken
                        );
                    break;
                case ConnectionEnum.Dataverse:
                    // await getAuthToken(crmClientId, crmClientSecret, crmScope, TENANT_ID)
                    break;
                case ConnectionEnum.OpenAI:
                    break;
                default:
                    throw new InvalidDataException("Connection not supported");
            }

            var response = await httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            return responseContent;
        }

        public async Task<T> SendHttpPost<T>(
            Uri relativeUri,
            string payload,
            ConnectionEnum connectionEnum,
            CancellationToken cancellationToken
        )
        {
            var httpClient = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, relativeUri);
            string accessToken = await _microsoftAuth.GetAccessToken(
                connectionEnum,
                cancellationToken
            );

            switch (connectionEnum)
            {
                case ConnectionEnum.Graph:

                    break;
                case ConnectionEnum.Dataverse:
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        accessToken
                    );
                    request.Headers.Add("Prefer", "return=representation");
                    request.Headers.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );

                    break;
                case ConnectionEnum.OpenAI:
                    break;
                default:
                    throw new InvalidDataException("Connection not supported");
            }
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            request.Content = content;

            var response = await httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            return (T)(object)responseContent;
        }



        public async Task<T> SendHttpPostAsync<T>(
            object payload,
            ConnectionEnum connectionEnum,
            string question,
            CancellationToken cancellationToken
        )
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                switch (connectionEnum)
                {
                    case ConnectionEnum.Graph:
                        break;
                    case ConnectionEnum.Dataverse:
                        break;
                    case ConnectionEnum.OpenAI:
                        httpClient.DefaultRequestHeaders.Add("api-key", API_KEY);
                        httpClient.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json")
                        );
                        httpClient.DefaultRequestHeaders.AcceptEncoding.Add(
                            new StringWithQualityHeaderValue("utf-8")
                        );
                        break;
                    default:
                        throw new InvalidDataException("Connection not supported");
                }
                return await _executeWithRetry.Execute(
                    async () =>
                    {
                        var response = await httpClient.PostAsync(
                            ENDPOINT,
                            new StringContent(
                                JsonSerializer.Serialize(payload),
                                Encoding.UTF8,
                                "application/json"
                            )
                        );
                        response.EnsureSuccessStatusCode();

                        var responseContent = await response.Content.ReadAsStringAsync();
                        using var document = JsonDocument.Parse(responseContent);
                        var root = document.RootElement;
                        var content = root.GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

                        // Fix CS0029: Convert string to T if possible, otherwise throw
                        // Fix CS8603: Ensure non-null return
                        if (content is null)
                            throw new InvalidOperationException("Response content is null.");

                        if (typeof(T) == typeof(string))
                        {
                            return (T)(object)content;
                        }
                        else
                        {
                            // Try to deserialize to T
                            return JsonSerializer.Deserialize<T>(content)
                                ?? throw new InvalidOperationException("Deserialization returned null.");
                        }
                    },
                    "HttpPost",
                    60,
                    3,
                    cancellationToken
                );
            }
        }
    }
}
