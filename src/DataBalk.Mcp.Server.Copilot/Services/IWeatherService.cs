using DataBalk.Mcp.Common.Model;

namespace DataBalk.Mcp.Server.Copilot.Services
{
    public interface IWeatherService
    {
        Task<WeatherData> GetWeatherAsync(string location);
        Task<WeatherForecast[]> GetForecastAsync(string location, int days = 5);
    }

}
