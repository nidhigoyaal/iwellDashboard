using BatteryDashboard.Server.Data;
using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BatteryDashboard.Server.Repository
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        private readonly AppDbContext context = context;

        public async Task<User?> GetUserByEmail(string userEmail)
        {
            return await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == userEmail);
        }

        public async Task<User> AddUser(User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExists(string email)
        {
            return await context.Users.AsNoTracking().AnyAsync(u => u.UserEmail.Equals(email, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
