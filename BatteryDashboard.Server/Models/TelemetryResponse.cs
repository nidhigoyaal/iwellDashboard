namespace BatteryDashboard.Server.Models
{
    public class TelemetryResponse
    {
        public List<Series> Series { get; set; } = new();
    }

    public class Series
    {
        public string Name { get; set; }
        public List<List<object>> Data { get; set; } = new();
    }
}
