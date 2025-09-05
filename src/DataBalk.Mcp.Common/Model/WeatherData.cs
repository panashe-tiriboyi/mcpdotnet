namespace DataBalk.Mcp.Common.Model
{

    public class WeatherData
    {
        public string Location { get; set; } = string.Empty;
        public int Temperature { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Humidity { get; set; }
        public int WindSpeed { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
