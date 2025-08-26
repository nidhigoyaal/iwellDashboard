using BatteryDashboard.Server.Models;

namespace BatteryDashboard.Server.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
