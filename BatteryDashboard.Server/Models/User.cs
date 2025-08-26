using System.ComponentModel.DataAnnotations;

namespace BatteryDashboard.Server.Models
{
    public class User
    {
        [Key]
        public string? UserId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";
    }
}
