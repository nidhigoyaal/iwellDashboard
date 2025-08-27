using BatteryDashboard.Server.Extensions;
using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace BatteryDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(IAccountService authService, ILogger<AccountController> logger, TelemetryClient telemetry) : ControllerBase
    {
        private readonly IAccountService authService = authService;
        private readonly ILogger<AccountController> logger = logger;
        private readonly TelemetryClient telemetry = telemetry;

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="register">The registration details including email and password.</param>
        /// <returns>An authentication token if registration is successful.</returns>
        /// <response code="200">Registration successful, returns a JWT token.</response>
        /// <response code="409">Registration failed, email already exists.</response>
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

        /// <summary>
        /// Logs in a user with provided credentials.
        /// </summary>
        /// <param name="login">The login details including email and password.</param>
        /// <returns>An authentication token if login is successful.</returns>
        /// <response code="200">Login successful, returns a JWT token.</response>
        /// <response code="401">Login failed, invalid email or password.</response>
        /// <response code="500">An unexpected server error occurred.</response>
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