namespace BatteryDashboard.Server.Extensions
{
    public static class StringExtensions
    {
        public static string Mask(this string input, int startVisible = 2, int endVisible = 2)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= startVisible + endVisible)
                return new string('*', input?.Length ?? 0);

            var maskedLength = input.Length - startVisible - endVisible;
            return input.Substring(0, startVisible) +
                   new string('*', maskedLength) +
                   input.Substring(input.Length - endVisible);
        }
    }
}
