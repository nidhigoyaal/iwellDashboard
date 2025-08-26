using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BatteryDashboard.Server.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(Config.Config.SqlConnectionString,
                sqlOptions => sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,               // number of retries
        maxRetryDelay: TimeSpan.FromSeconds(10), // wait time between retries
        errorNumbersToAdd: null          // optional: specify SQL error codes to retry
    ));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
