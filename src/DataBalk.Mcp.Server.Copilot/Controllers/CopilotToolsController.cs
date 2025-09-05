using DataBalk.Mcp.Common.Model;
using DataBalk.Mcp.Common.Services.HttpConnector;
using Mcp.Net.Core.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace DataBalk.Mcp.Server.Copilot.Controllers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class McpResourceAttribute : Attribute
    {
    }

    [ApiController]
    [Route("mcp")]
    public class CopilotToolsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpRequestConnector _httpConnector;
        private readonly string _dataverseUrl;
        private readonly string _accessToken;

        public CopilotToolsController(
            IConfiguration configuration,
            IHttpRequestConnector httpConnector
           )
        {
            _configuration = configuration;
            _httpConnector = httpConnector;
            _dataverseUrl = _configuration["CRM_BASE_URL"];

        }

        [HttpGet("GetWorkItems")]
        [McpTool("GetWorkItems", "Get Azure DevOps work items")]
        [Description("Retrieves work items from Azure DevOps")]
        public async Task<IActionResult> GetWorkItems(
            [FromQuery][Description("Project name in Azure DevOps")] string project,
            [FromQuery][Description("Work item type (Bug, Task, etc.)")] string workItemType = "Task",
            [FromQuery][Description("Maximum number of items to retrieve")] int top = 10)
        {
            try
            {
                var organizationUrl = _configuration["AzureDevOps:OrganizationUrl"];
                var wiqlUri = new Uri($"{organizationUrl}/{project}/_apis/wit/wiql?api-version=7.0");

                var wiqlQuery = $@"SELECT [System.Id] 
                                  FROM WorkItems 
                                  WHERE [System.TeamProject] = '{project}' 
                                  AND [System.WorkItemType] = '{workItemType}'
                                  ORDER BY [System.CreatedDate] DESC";

                var payload = new { query = wiqlQuery };

                var result = await _httpConnector.SendHttpPost<WorkItemQueryResponse>(
                    wiqlUri,
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    ConnectionEnum.AzureDevOps,
                    CancellationToken.None
                );

                if (result == null)
                    return Ok(Array.Empty<object>());

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving work items: {ex.Message}");
            }
        }

        [HttpGet("GetAccounts")]
        [McpTool("GetAccounts", "Get Dataverse Account records")]
        [Description("Retrieves account records from Dataverse")]
        public async Task<IActionResult> GetAccounts(
            [FromQuery][Description("Optional query string to filter accounts")] string? query,
            CancellationToken cancellationToken)
        {
            try
            {
                var baseUri = $"{_dataverseUrl}accounts?$top=20";
                //if (!string.IsNullOrWhiteSpace(query))
                //{
                //    baseUri += $"&$filter=contains(name,'{Uri.EscapeDataString(query)}')";
                //}

                var uri = new Uri(baseUri);
                var result = await _httpConnector.SendHttpGet<OdataResponse<AccountResponse>>(
                    uri,
                    ConnectionEnum.Dataverse,
                    cancellationToken
                );

                if (result == null)
                    return Ok(Array.Empty<object>());

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving accounts: {ex.Message}");
            }
        }
    }
}