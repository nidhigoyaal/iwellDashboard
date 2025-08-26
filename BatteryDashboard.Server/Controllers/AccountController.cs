using BatteryDashboard.Server.Extensions;
using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace BatteryDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(IAccountService authService, ILogger<BatteryController> logger, TelemetryClient telemetry) : ControllerBase
    {
        private readonly IAccountService authService = authService;
        private readonly ILogger<BatteryController> logger = logger;
        private readonly TelemetryClient telemetry = telemetry;

        [HttpPost("register")]
        public async Task<IActionResult> Register(Register register)
        {
            var token = await authService.Register(register);

            if (token == null)
            {
                logger.LogWarning("Registration failed: email already exists {Email}", register.UserEmail.Mask());
                telemetry.TrackEvent("RegisterFailed_UserExists", new Dictionary<string, string>
                    {
                        { "Email", register.UserEmail.Mask() }
                    });

                return Conflict(new { message = "Email already exists." });
            }

            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            try
            {
                var token = await authService.Login(login);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogWarning("Login failed for {Email}", login.UserEmail.Mask());
                telemetry.TrackEvent("LoginFailed", new Dictionary<string, string>
                {
                    { "Email", login.UserEmail.Mask() }
                });

                return Unauthorized();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during login for {Email}", login.UserEmail.Mask());
                telemetry.TrackException(ex);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}