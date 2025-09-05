using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace DataBalk.Mcp.Common.Services.MicrosoftAuth
{
    public class MicrosoftAuth : IMicrosoftAuth
    {
        private readonly IConfiguration _configuration;
        private readonly string TENANT_ID;
        private readonly string graphClientId;
        private readonly string graphClientSecret;
        private readonly string graphScope;
        private readonly string crmClientId;
        private readonly string crmClientSecret;
        private readonly string crmScope;

        public MicrosoftAuth(IConfiguration configuration)
        {
            _configuration = configuration;
            graphClientId = _configuration["GRAPH:ClientId"];
            graphClientSecret = _configuration["GRAPH:ClientSecret"];
            graphScope = _configuration["GRAPH:Scope"];
            crmClientId = _configuration["CRM_CLIENT_ID"];
            crmClientSecret = _configuration["CRM_CLIENT_SECRET"];
            crmScope = _configuration["CRM_SCOPE"];
            TENANT_ID = _configuration["TENANT_ID"];
        }

        public async Task<string> GetAccessToken(
            ConnectionEnum connection,
            CancellationToken cancellation
        )
        {
            switch (connection)
            {
                case ConnectionEnum.Graph:
                    return (
                        await getAuthToken(graphClientId, graphClientSecret, graphScope, TENANT_ID)
                    ).AccessToken;
                case ConnectionEnum.Dataverse:
                    return (
                        await getAuthToken(crmClientId, crmClientSecret, crmScope, TENANT_ID)
                    ).AccessToken;
                // case ConnectionEnum.OpenAI:
                // return await getAuthToken();
                default:
                    throw new InvalidDataException("Connection not supported");
            }
        }

        private async Task<AuthenticationResult> getAuthToken(
            string clientID,
            string secret,
            string scope,
            string TENANT_ID
        )
        {
            var app = ConfidentialClientApplicationBuilder
                .Create(clientID)
                .WithClientSecret(secret)
                .WithAuthority(
                    new Uri($"https://login.microsoftonline.com/{TENANT_ID}/oauth2/v2.0/token")
                )
                .Build();
            return await app.AcquireTokenForClient(new[] { scope }).ExecuteAsync();
        }
    }
}
