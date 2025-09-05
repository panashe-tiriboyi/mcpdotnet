using System.Text.Json.Serialization;

public class WorkItemQueryResponse
{
    [JsonPropertyName("queryType")]
    public string QueryType { get; set; }

    [JsonPropertyName("queryResultType")]
    public string QueryResultType { get; set; }

    [JsonPropertyName("asOf")]
    public DateTime AsOf { get; set; }

    [JsonPropertyName("columns")]
    public List<WorkItemColumn> Columns { get; set; }

    [JsonPropertyName("workItems")]
    public List<WorkItemReference> WorkItems { get; set; }
}
