using Microsoft.EntityFrameworkCore;
using BatteryDashboard.Server.Models;

namespace BatteryDashboard.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}