using BatteryDashboard.Server.Extensions;
using BatteryDashboard.Server.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BatteryDashboard.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class BatteryController(HttpClient httpClient, ILogger<BatteryController> logger, TelemetryClient telemetry) : ControllerBase
    {
        private readonly HttpClient httpClient = httpClient;
        private readonly ILogger<BatteryController> logger = logger;
        private readonly TelemetryClient telemetry = telemetry;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        [HttpGet("{deviceId}/status"), Authorize]
        public async Task<IActionResult> GetBatteryStatus(string deviceId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{Config.Config.IWellApi}/api/v1/batteries/{deviceId}/status");
                request.Headers.Add("x-api-key", Config.Config.IWellApiKey);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                logger.LogInformation("Fetched battery status for device {DeviceId}", deviceId.Mask());
                telemetry.TrackEvent("BatteryStatusFetched", new Dictionary<string, string>
                {
                    { "DeviceId", deviceId.Mask() }
                });

                return Ok(content);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error fetching battery status for device {DeviceId}", deviceId.Mask());
                telemetry.TrackException(ex);
                return StatusCode(500, "Error fetching battery status.");
            }
        }

        [HttpGet("{deviceId}/telemetry"), Authorize]
        public async Task<IActionResult> GetTelemetry(string deviceId, [FromQuery] int offsetMinutes)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{Config.Config.IWellApi}/api/v1/batteries/{deviceId}/telemetry?OffsetMinutes={offsetMinutes}");
                request.Headers.Add("x-api-key", Config.Config.IWellApiKey);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var allSeries = JsonSerializer.Deserialize<TelemetryResponse>(content, _jsonOptions);

                if (allSeries == null) return Ok(new { series = new List<Series>() });

                IEnumerable<Series> filtered = allSeries.Series.Where(s => s.Name == "BatteryPowerW" || s.Name == "GridPowerW");

                logger.LogInformation("Fetched telemetry for device {DeviceId}, OffsetMinutes={OffsetMinutes}", deviceId.Mask(), offsetMinutes);
                telemetry.TrackEvent("TelemetryFetched", new Dictionary<string, string>
                {
                    { "DeviceId", deviceId.Mask() },
                    { "OffsetMinutes", offsetMinutes.ToString() }
                });

                return Ok(new { series = filtered });
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error fetching telemetry for device {DeviceId}", deviceId.Mask());
                telemetry.TrackException(ex);
                return StatusCode(500, "Error fetching telemetry.");
            }
        }
    }
}
