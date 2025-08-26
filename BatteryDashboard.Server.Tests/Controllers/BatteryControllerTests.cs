using BatteryDashboard.Server.Controllers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BatteryDashboard.Server.Tests.Controllers;
public class BatteryControllerTests
{
    private readonly Mock<ILogger<BatteryController>> _loggerMock;
    private readonly TelemetryClient _telemetry;
    private readonly HttpClient _httpClient;
    private readonly BatteryController _controller;

    public BatteryControllerTests()
    {
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(),
            DisableTelemetry = true // Prevents sending data
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, System.Threading.CancellationToken ct) =>
            {
                var content = "{}"; // empty JSON response
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };
            });

        _loggerMock = new Mock<ILogger<BatteryController>>();
        _telemetry = _telemetry = new TelemetryClient(telemetryConfiguration);
        _httpClient = new HttpClient(handlerMock.Object);
        _controller = new BatteryController(_httpClient, _loggerMock.Object, _telemetry);
    }

    [Fact]
    public async Task GetBatteryStatus_ReturnsOk()
    {
        Config.Config.IWellApi = "https://mock-api";
        Config.Config.IWellApiKey = "test-key";
        var result = await _controller.GetBatteryStatus("device123") as ObjectResult;
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task GetTelemetry_ReturnsOkWithFilteredSeries()
    {
        Config.Config.IWellApi = "https://mock-api";
        Config.Config.IWellApiKey = "test-key";
        var seriesJson = JsonSerializer.Serialize(new
        {
            Series = new[]
            {
                new { Name = "BatteryPowerW", Values  = new int[] {1,2,3} },
                new { Name = "GridPowerW", Values  = new int[] {4,5,6} }
            }
        });
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(seriesJson, Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handlerMock.Object);
        var controller = new BatteryController(client, _loggerMock.Object, _telemetry);

        var result = await controller.GetTelemetry("device123", 60) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }
}
