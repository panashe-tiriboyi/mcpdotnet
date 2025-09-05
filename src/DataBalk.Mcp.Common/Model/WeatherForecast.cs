namespace DataBalk.Mcp.Common.Model
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public int HighTemp { get; set; }
        public int LowTemp { get; set; }
        public string Description { get; set; } = string.Empty;
        public int ChanceOfRain { get; set; }
    }
}
