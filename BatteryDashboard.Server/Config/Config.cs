using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace BatteryDashboard.Server.Config
{
    public static class Config
    {
        public static string? IWellApiKey { get; private set; }
        public static string? IWellApi { get; private set; }

        public static string? SqlConnectionString { get; private set; }
        public static string? JwtKey { get; private set; }
        public static async Task Load(ConfigurationManager config)
        {
            string? keyVaultName = config.GetValue<string>("KeyVaultName");
            var client = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), new DefaultAzureCredential());

            KeyVaultSecret iWellApiKey = await client.GetSecretAsync("IWellApiKey");
            IWellApiKey = iWellApiKey.Value;

            KeyVaultSecret iWellApi = await client.GetSecretAsync("IWellApi");
            IWellApi = iWellApi.Value;

            KeyVaultSecret sqlConnectionString = await client.GetSecretAsync("SqlConnectionString");
            SqlConnectionString = sqlConnectionString.Value;

            KeyVaultSecret jwtKey = await client.GetSecretAsync("Jwt--Key");
            JwtKey = jwtKey.Value;
        }
    }
}
