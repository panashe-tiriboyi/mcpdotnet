using System.Text.Json.Serialization;

namespace DataBalk.Mcp.Common.Model
{
    public class AccountResponse
    {

        [JsonPropertyName("_ownerid_value")]
        public string? OwnerId { get; set; }

        [JsonPropertyName("address1_country")]
        public string address1_country { get; set; }

        [JsonPropertyName("_owninguser_value")]
        public string? OwningUser { get; set; }

        [JsonPropertyName("statecode")]
        public int Statecode { get; set; } = 0;

        [JsonPropertyName("address1_line1")]
        public string Address1_line1 { get; set; }

        [JsonPropertyName("address2_addressid")]
        public string Address2Id { get; set; }

        [JsonPropertyName("address1_addressid")]
        public string? Address1Id { get; set; }

        [JsonPropertyName("address1_city")]
        public string? Address1_city { get; set; }

        [JsonPropertyName("address1_composite")]
        public string? address1_composite { get; set; }

        [JsonPropertyName("accountid")]
        public string? Accountid { get; set; }
        [JsonPropertyName("emailaddress1")]
        public string? Emailaddress1 { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("createdon")]
        public string? Createdon { get; set; }
    }
}
