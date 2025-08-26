using BatteryDashboard.Server.Models;

namespace BatteryDashboard.Server.Interfaces
{
    public interface IAccountService
    {
        Task<string?> Login(Login login);
        Task<string?> Register(Register register);
    }
}
