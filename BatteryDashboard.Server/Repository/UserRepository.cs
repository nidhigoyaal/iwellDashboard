using BatteryDashboard.Server.Data;
using BatteryDashboard.Server.Extensions;
using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;

namespace BatteryDashboard.Server.Repository
{
    public class UserRepository(AppDbContext context, ILogger<UserRepository> logger, TelemetryClient telemetry) : IUserRepository
    {
        private readonly AppDbContext context = context;
        private readonly ILogger<UserRepository> logger = logger;
        private readonly TelemetryClient telemetry = telemetry;

        public async Task<User?> GetUserByEmail(string userEmail)
        {
            try
            {
                return await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error fetching user by email {Email}", userEmail.Mask());
                telemetry.TrackException(ex);
                throw;
            }
        }

        public async Task<User> AddUser(User user)
        {
            try
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();

                logger.LogInformation("User added successfully: {Email}", user.UserEmail.Mask());
                telemetry.TrackEvent("UserAdded", new Dictionary<string, string>
                {
                    { "Email", user.UserEmail.Mask() }
                });

                return user;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error adding user {Email}", user.UserEmail.Mask());
                telemetry.TrackException(ex);
                throw;
            }
        }

        public async Task<bool> UserExists(string email)
        {
            try
            {
                return await context.Users.AsNoTracking().AnyAsync(u => u.UserEmail == email);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error checking user existence {Email}", email.Mask());
                telemetry.TrackException(ex);
                throw;
            }
        }
    }
}
