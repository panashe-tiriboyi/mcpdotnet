using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // OpenAI setup
        const string endpoint = "https://mul-prod-azureai-open-teamsviva-bi.openai.azure.com/openai/v1/";
        const string apiKey = "226cf61741cb4288b8926d28dea0b3a3";
        const string model = "gpt-4o";

        ChatClient client = new(
            credential: new ApiKeyCredential(apiKey),
            model: model,
            options: new OpenAIClientOptions() { Endpoint = new($"{endpoint}") }
        );

        // Define tools
        var options = new ChatCompletionOptions();

        options.Tools.Add(ChatTool.CreateFunctionTool(
            "get_work_items",
            "Get work items from Azure DevOps",
            BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "project": {"type": "string", "description": "Project name"},
                    "workItemType": {"type": "string", "default": "Task"},
                    "top": {"type": "integer", "default": 10}
                },
                "required": ["project"]
            }
            """)
        ));

        options.Tools.Add(ChatTool.CreateFunctionTool(
            "get_accounts",
            "Get accounts with optional filter",
            BinaryData.FromString("""
            {
                "type": "object", 
                "properties": {
                    "query": {"type": "string", "description": "Filter query"}
                }
            }
            """)
        ));

        // Chat messages
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You can access work items and accounts. Use tools when needed. Format responses in a user-friendly way."),
            new UserChatMessage("Show me all accounts")
        };

        // Get completion
        ChatCompletion completion = client.CompleteChat(messages, options);

        // Handle response
        Console.WriteLine($"Role: {completion.Role}");

        // Check if there are tool calls
        if (completion.ToolCalls?.Count > 0)
        {
            foreach (var toolCall in completion.ToolCalls)
            {
                Console.WriteLine($"Tool: {toolCall.FunctionName}");
                Console.WriteLine($"Args: {toolCall.FunctionArguments}");

                // Call MCP server
                string result = await CallMcpServer(toolCall.FunctionName, toolCall.FunctionArguments.ToString());

                // Format the response
                var formattedResponse = FormatResponse(toolCall.FunctionName, result);
                Console.WriteLine("\nFormatted Response:");
                Console.WriteLine(formattedResponse);
            }
        }
        else
        {
            // Regular text response
            foreach (var part in completion.Content)
            {
                Console.WriteLine($"Response: {part.Text}");
            }
        }
    }

    static async Task<string> CallMcpServer(string functionName, string args)
    {
        const string baseUrl = "https://46c76076eab5.ngrok-free.app";  // Removed /mcp from base URL

        Console.WriteLine($"DEBUG: Received function call: {functionName}");
        Console.WriteLine($"DEBUG: Arguments: {args}");

        var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(args);
        if (parameters == null)
        {
            throw new ArgumentException("Failed to deserialize function arguments", nameof(args));
        }

        string url = functionName switch
        {
            "get_work_items" => $"{baseUrl}/mcp/GetWorkItems?project={Uri.EscapeDataString(parameters.GetValueOrDefault("project")?.ToString() ?? "")}&workItemType={Uri.EscapeDataString(parameters.GetValueOrDefault("workItemType", "Task")?.ToString() ?? "")}&top={parameters.GetValueOrDefault("top", 10)}",
            "get_accounts" => $"{baseUrl}/mcp/GetAccounts" + (parameters.ContainsKey("query") ? $"?query={Uri.EscapeDataString(parameters["query"]?.ToString() ?? "")}" : ""),
            _ => throw new ArgumentException($"Unknown function name: {functionName}", nameof(functionName))
        };

        Console.WriteLine($"DEBUG: Calling URL: {url}");

        try
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"DEBUG: Response Status: {response.StatusCode}");
            Console.WriteLine($"DEBUG: Response Content: {content}");

            response.EnsureSuccessStatusCode();
            return content;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"DEBUG: Exception occurred: {ex.Message}");
            Console.WriteLine($"DEBUG: Inner exception: {ex.InnerException?.Message}");
            throw;
        }
    }

    static string FormatResponse(string functionName, string jsonResponse)
    {
        try
        {
            var response = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            switch (functionName)
            {
                case "get_accounts":
                    var sb = new StringBuilder();
                    sb.AppendLine("Here are the accounts I found:");
                    sb.AppendLine();

                    if (response.ValueKind == JsonValueKind.Object)
                    {
                        // Format a single account
                        FormatAccountInfo(sb, response);
                    }
                    else if (response.ValueKind == JsonValueKind.Array)
                    {
                        // Format multiple accounts
                        foreach (var account in response.EnumerateArray())
                        {
                            FormatAccountInfo(sb, account);
                            sb.AppendLine("-------------------");
                        }
                    }

                    return sb.ToString();

                case "get_work_items":
                    // Add work items formatting when implemented
                    return "Work items formatting not yet implemented.";

                default:
                    return "Unknown response type.";
            }
        }
        catch (JsonException ex)
        {
            return $"Error parsing response: {ex.Message}\nRaw response: {jsonResponse}";
        }
    }

    static void FormatAccountInfo(StringBuilder sb, JsonElement account)
    {
        // Try to get common account properties, skip if not found
        if (account.TryGetProperty("name", out var name))
            sb.AppendLine($"Account Name: {name}");

        if (account.TryGetProperty("accountnumber", out var accountNumber))
            sb.AppendLine($"Account Number: {accountNumber}");

        if (account.TryGetProperty("emailaddress1", out var email))
            sb.AppendLine($"Email: {email}");

        if (account.TryGetProperty("telephone1", out var phone))
            sb.AppendLine($"Phone: {phone}");

        if (account.TryGetProperty("address1_city", out var city))
            sb.AppendLine($"City: {city}");

        if (account.TryGetProperty("statecode", out var state))
        {
            string status = state.GetInt32() switch
            {
                0 => "Active",
                1 => "Inactive",
                _ => "Unknown"
            };
            sb.AppendLine($"Status: {status}");
        }
    }
}