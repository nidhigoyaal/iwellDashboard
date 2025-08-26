using BatteryDashboard.Server.Controllers;
using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BatteryDashboard.Server.Tests.Controllers;
public class AccountControllerTests
{
    private readonly Mock<IAccountService> _authServiceMock;
    private readonly Mock<ILogger<AccountController>> _loggerMock;
    private readonly TelemetryClient _telemetry;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(),
            DisableTelemetry = true // Prevents sending data
        };

        _authServiceMock = new Mock<IAccountService>();
        _loggerMock = new Mock<ILogger<AccountController>>();
        _telemetry = new TelemetryClient(telemetryConfiguration);

        _controller = new AccountController(
            _authServiceMock.Object,
            _loggerMock.Object,
            _telemetry
        );
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenTokenIsReturned()
    {
        // Arrange
        var register = new Register { UserEmail = "test@example.com", UserName = "Test", Password = "1234", Role = "User" };
        var fakeToken = "fake-token";
        _authServiceMock.Setup(a => a.Register(register)).ReturnsAsync(fakeToken);

        // Act
        var result = await _controller.Register(register);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        var tokenProperty = value!.GetType().GetProperty("token");

        Assert.NotNull(tokenProperty);
        Assert.Equal(fakeToken, tokenProperty.GetValue(value));
        Assert.Equal(fakeToken, tokenProperty.GetValue(value));
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenTokenIsNull()
    {
        // Arrange
        var register = new Register { UserEmail = "test@example.com", UserName = "Test", Password = "1234", Role = "User" };
        _authServiceMock.Setup(a => a.Register(register)).ReturnsAsync((string)null);

        // Act
        var result = await _controller.Register(register);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        var value = conflictResult.Value;
        var messageProperty = value!.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        var message = messageProperty.GetValue(value)?.ToString();

        Assert.Equal("Email already exists.", message);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Registration failed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenLoginSucceeds()
    {
        // Arrange
        var login = new Login { UserEmail = "test@example.com", Password = "1234" };
        var fakeToken = "fake-token";
        _authServiceMock.Setup(a => a.Login(login)).ReturnsAsync(fakeToken);

        // Act
        var result = await _controller.Login(login);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        var tokenProperty = value!.GetType().GetProperty("token");

        Assert.NotNull(tokenProperty);
        Assert.Equal(fakeToken, tokenProperty.GetValue(value));
        Assert.Equal(fakeToken, tokenProperty.GetValue(value));
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUnauthorizedAccessExceptionThrown()
    {
        // Arrange
        var login = new Login { UserEmail = "test@example.com", Password = "wrongpass" };
        _authServiceMock.Setup(a => a.Login(login)).ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.Login(login);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login failed")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldReturn500_WhenGenericExceptionThrown()
    {
        // Arrange
        var login = new Login { UserEmail = "test@example.com", Password = "1234" };
        var exception = new Exception("Something went wrong");
        _authServiceMock.Setup(a => a.Login(login)).ThrowsAsync(exception);

        // Act
        var result = await _controller.Login(login);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);

        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected error")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
