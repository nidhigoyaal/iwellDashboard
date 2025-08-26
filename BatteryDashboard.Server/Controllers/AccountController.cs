using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace BatteryDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(IAccountService authService) : ControllerBase
    {
        private readonly IAccountService authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(Register register)
        {
            var token = await authService.Register(register);

            if (token == null)
            {
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
            catch (UnauthorizedAccessException ex)
            {
                // You can log the exception here
                return Unauthorized();
            }
            catch (Exception)
            {
                // Handle other potential errors gracefully
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}