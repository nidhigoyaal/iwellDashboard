using BatteryDashboard.Server.Extensions;
using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.ApplicationInsights;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BatteryDashboard.Server.Services
{
    public class JwtTokenService(ILogger<JwtTokenService> logger, TelemetryClient telemetry) : IJwtTokenService
    {
        private readonly ILogger<JwtTokenService> logger = logger;
        private readonly TelemetryClient telemetry = telemetry;

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config.Config.JwtKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserEmail),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                 issuer: Config.Config.JwtIssuer,
                 audience: Config.Config.JwtAudience,
                 claims: claims,
                 expires: DateTime.UtcNow.AddHours(2),
                 signingCredentials: creds
             );

            logger.LogInformation("JWT token generated for user {Email}", user.UserEmail.Mask());
            telemetry.TrackEvent("JwtTokenGenerated", new Dictionary<string, string>
                {
                    { "Email", user.UserEmail.Mask() },
                    { "Expires", token.ValidTo.ToString("o") }
                });

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
