using System.Text.Json.Serialization;

public class WorkItemReference
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}