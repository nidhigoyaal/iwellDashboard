using BatteryDashboard.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BatteryDashboard.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class BatteryController(HttpClient httpClient) : ControllerBase
    {
        private readonly HttpClient httpClient = httpClient;


        [HttpGet("{deviceId}/status"), Authorize]
        public async Task<IActionResult> GetBatteryStatus(string deviceId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{Config.Config.IWellApi}/api/v1/batteries/{deviceId}/status");
            request.Headers.Add("x-api-key", Config.Config.IWellApiKey);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }

        [HttpGet("{deviceId}/telemetry"), Authorize]
        public async Task<IActionResult> GetTelemetry(string deviceId, [FromQuery] int offsetMinutes)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{Config.Config.IWellApi}/api/v1/batteries/{deviceId}/telemetry?OffsetMinutes={offsetMinutes}");
            request.Headers.Add("x-api-key", Config.Config.IWellApiKey);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var allSeries = JsonSerializer.Deserialize<TelemetryResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (allSeries == null) return Ok(new { series = new List<Series>() });

            IEnumerable<Series> filtered = allSeries.Series.Where(s => s.Name == "BatteryPowerW" || s.Name == "GridPowerW");

            return Ok(new { series = filtered });
        }
    }
}
