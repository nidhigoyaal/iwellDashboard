namespace BatteryDashboard.Server.Models
{
    public class Register
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}
