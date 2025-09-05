using System.Text.Json.Serialization;

public class WorkItemColumn
{
    [JsonPropertyName("referenceName")]
    public string ReferenceName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}
