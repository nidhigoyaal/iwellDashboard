using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using BatteryDashboard.Server.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace BatteryDashboard.Server.Tests.Services;
public class AccountServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly TelemetryClient _telemetry;
    private readonly Mock<ILogger<AccountService>> _loggerMock;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        var telemetryConfig = new TelemetryConfiguration
        {
            TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(),
            DisableTelemetry = true // prevent sending
        };

        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<AccountService>>();
        _telemetry = new TelemetryClient(telemetryConfig);

        _service = new AccountService(_userRepoMock.Object, _jwtMock.Object, _telemetry, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenUserExistsAndPasswordCorrect()
    {
        var login = new Login { UserEmail = "a@b.com", Password = "pass123" };
        var user = new User
        {
            UserEmail = login.UserEmail,
            PasswordHash = new PasswordHasher<User>().HashPassword(null, login.Password)
        };

        _userRepoMock.Setup(x => x.GetUserByEmail(login.UserEmail))
            .ReturnsAsync(user);

        _jwtMock.Setup(x => x.GenerateToken(user)).Returns("FAKE_TOKEN");

        var token = await _service.Login(login);

        Assert.Equal("FAKE_TOKEN", token);
    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        var login = new Login { UserEmail = "missing@b.com", Password = "123" };
        _userRepoMock.Setup(x => x.GetUserByEmail(login.UserEmail)).ReturnsAsync((User)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.Login(login));
    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorized_WhenPasswordIncorrect()
    {
        var login = new Login { UserEmail = "a@b.com", Password = "wrongpass" };
        var user = new User
        {
            UserEmail = login.UserEmail,
            PasswordHash = new PasswordHasher<User>().HashPassword(null, "correctpass")
        };

        _userRepoMock.Setup(x => x.GetUserByEmail(login.UserEmail)).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.Login(login));
    }

    [Fact]
    public async Task Register_ShouldReturnToken_WhenUserDoesNotExist()
    {
        var register = new Register
        {
            UserEmail = "new@b.com",
            UserName = "NewUser",
            Password = "1234",
            Role = "User",
            Id = Guid.NewGuid().ToString()
        };

        _userRepoMock.Setup(x => x.UserExists(register.UserEmail)).ReturnsAsync(false);
        _jwtMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("FAKE_TOKEN");

        var token = await _service.Register(register);

        Assert.Equal("FAKE_TOKEN", token);
        _userRepoMock.Verify(x => x.AddUser(It.Is<User>(u => u.UserEmail == register.UserEmail)), Times.Once);
    }

    [Fact]
    public async Task Register_ShouldReturnNull_WhenUserAlreadyExists()
    {
        var register = new Register
        {
            UserEmail = "existing@b.com",
            UserName = "User",
            Password = "1234",
            Role = "User"
        };

        _userRepoMock.Setup(x => x.UserExists(register.UserEmail)).ReturnsAsync(true);

        var token = await _service.Register(register);

        Assert.Null(token);
        _userRepoMock.Verify(x => x.AddUser(It.IsAny<User>()), Times.Never);
    }
}
