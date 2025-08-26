using BatteryDashboard.Server.Extensions;
using BatteryDashboard.Server.Models;
using BatteryDashboard.Server.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace BatteryDashboard.Server.Tests.Services;
public class JwtTokenServiceTests
{
    private readonly Mock<ILogger<JwtTokenService>> _loggerMock;
    private readonly TelemetryClient _telemetry;
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        var telemetryConfig = new TelemetryConfiguration
        {
            TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(),
            DisableTelemetry = true
        };
        _telemetry = new TelemetryClient(telemetryConfig);
        _loggerMock = new Mock<ILogger<JwtTokenService>>();

        _service = new JwtTokenService(_loggerMock.Object, _telemetry);
    }

    [Fact]
    public void GenerateToken_ShouldReturnJwtToken_WithCorrectClaims()
    {
        // Arrange
        Config.Config.JwtKey = "TestSecretKey1234567890ABCDEFGH!";
        Config.Config.JwtIssuer = "TestIssuer";
        Config.Config.JwtAudience = "TestAudience";
        var user = new User
        {
            UserEmail = "test@example.com",
            UserName = "TestUser",
            Role = "Admin"
        };

        // Act
        var tokenStr = _service.GenerateToken(user);

        // Assert
        Assert.False(string.IsNullOrEmpty(tokenStr));

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenStr);

        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.UserEmail);
        Assert.Contains(token.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Name && c.Value == user.UserName);
        Assert.Contains(token.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == user.Role);
        Assert.True(token.ValidTo > DateTime.UtcNow);

        var maskedEmail = user.UserEmail.Mask();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(maskedEmail)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
