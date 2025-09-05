using DataBalk.Mcp.Common.Model;

namespace DataBalk.Mcp.Server.Copilot.Services
{
    // Weather service implementation
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<WeatherData> GetWeatherAsync(string location)
        {
            // Mock implementation - replace with actual weather API
            await Task.Delay(100); // Simulate API call

            var random = new Random();
            return new WeatherData
            {
                Location = location,
                Temperature = random.Next(-10, 35),
                Description = GetRandomWeatherDescription(),
                Humidity = random.Next(20, 90),
                WindSpeed = random.Next(0, 30),
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<WeatherForecast[]> GetForecastAsync(string location, int days = 5)
        {
            // Mock implementation - replace with actual weather API
            await Task.Delay(200); // Simulate API call

            var random = new Random();
            var forecast = new WeatherForecast[days];

            for (int i = 0; i < days; i++)
            {
                forecast[i] = new WeatherForecast
                {
                    Date = DateTime.Today.AddDays(i),
                    Location = location,
                    HighTemp = random.Next(15, 35),
                    LowTemp = random.Next(-5, 20),
                    Description = GetRandomWeatherDescription(),
                    ChanceOfRain = random.Next(0, 100)
                };
            }

            return forecast;
        }

        private string GetRandomWeatherDescription()
        {
            var descriptions = new[] { "Sunny", "Partly Cloudy", "Cloudy", "Rainy", "Stormy", "Snowy", "Foggy" };
            return descriptions[new Random().Next(descriptions.Length)];
        }
    }

}
