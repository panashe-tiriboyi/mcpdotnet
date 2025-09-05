using System.Text.Json.Serialization;

namespace DataBalk.Mcp.Common.Model;

public class OdataResponse<T>
{
    [JsonPropertyName("value")]
    public List<T>? Value { get; set; }

    [JsonPropertyName("@odata.nextLink")]
    public string? OdataNextLink { get; set; }
}
