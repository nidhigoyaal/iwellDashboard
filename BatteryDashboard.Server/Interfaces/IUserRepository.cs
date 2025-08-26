using BatteryDashboard.Server.Models;

namespace BatteryDashboard.Server.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmail(string userEmail);
        Task<User> AddUser(User user);
        Task<bool> UserExists(string email);
    }
}
