using BatteryDashboard.Server.Extensions;
using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Identity;

namespace BatteryDashboard.Server.Services
{
    public class AccountService(IUserRepository userRepository, IJwtTokenService jwtService, TelemetryClient telemetry, ILogger<AccountService> logger) : IAccountService
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IJwtTokenService jwtService = jwtService;
        private readonly PasswordHasher<User> passwordHasher = new();
        private readonly TelemetryClient telemetry = telemetry;
        private readonly ILogger<AccountService> logger = logger;

        public async Task<string?> Login(Login login)
        {
            try
            {
                var user = await userRepository.GetUserByEmail(login.UserEmail);

                if (user == null)
                {
                    logger.LogWarning("Login failed for email {Email}: user not found", login.UserEmail.Mask());
                    telemetry.TrackEvent("LoginFailed_UserNotFound", new Dictionary<string, string>
                    {
                        { "Email", login.UserEmail.Mask() }
                    });
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);

                if (verificationResult != PasswordVerificationResult.Success)
                {
                    logger.LogWarning("Login failed for email {Email}: incorrect password", login.UserEmail.Mask());
                    telemetry.TrackEvent("LoginFailed_IncorrectPassword", new Dictionary<string, string>
                    {
                        { "Email", login.UserEmail.Mask() }
                    });
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }
                logger.LogInformation("User {Email} logged in successfully", login.UserEmail.Mask());
                telemetry.TrackEvent("LoginSuccess", new Dictionary<string, string>
                {
                    { "Email", login.UserEmail.Mask() }
                });

                return jwtService.GenerateToken(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login for {Email}", login.UserEmail.Mask());
                telemetry.TrackException(ex);
                throw;
            }
        }

        public async Task<string?> Register(Register register)
        {
            try
            {
                if (await userRepository.UserExists(register.UserEmail))
                {
                    logger.LogWarning("Registration failed: user already exists {Email}", register.UserEmail.Mask());
                    telemetry.TrackEvent("RegisterFailed_UserExists", new Dictionary<string, string>
               {
                    { "Email", register.UserEmail.Mask() }
               });
                    return null;
                }

                User newUser = new()
                {
                    UserId = register.Id,
                    UserEmail = register.UserEmail,
                    UserName = register.UserName,
                    Role = register.Role
                };

                newUser.PasswordHash = passwordHasher.HashPassword(newUser, register.Password);

                await userRepository.AddUser(newUser);

                logger.LogInformation("User {Email} registered successfully", register.UserEmail.Mask());
                telemetry.TrackEvent("RegisterSuccess", new Dictionary<string, string>
            {
                    { "Email", register.UserEmail.Mask() }
            });

                return jwtService.GenerateToken(newUser);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error during registration for {Email}", register.UserEmail.Mask());
                telemetry.TrackException(ex);
                throw;
            }
        }
    }
}
