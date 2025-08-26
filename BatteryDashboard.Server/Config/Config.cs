using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BatteryDashboard.Server.Tests")]
namespace BatteryDashboard.Server.Config
{
    public static class Config
    {
        public static string? IWellApiKey { get; internal set; }
        public static string? IWellApi { get; internal set; }

        public static string? SqlConnectionString { get; internal set; }
        public static string? JwtKey { get; internal set; }
        public static string? JwtIssuer { get; internal set; }
        public static string? JwtAudience { get; internal set; }

        public static async Task Load(ConfigurationManager config)
        {
            string? keyVaultName = config.GetValue<string>("KeyVaultName");
            var client = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), new DefaultAzureCredential());

            KeyVaultSecret iWellApiKey = await client.GetSecretAsync("IWellApiKey");
            IWellApiKey = iWellApiKey.Value;

            KeyVaultSecret iWellApi = await client.GetSecretAsync("IWellApi");
            IWellApi = iWellApi.Value;

            KeyVaultSecret sqlConnectionString = await client.GetSecretAsync("DefaultConnection");
            SqlConnectionString = sqlConnectionString.Value;

            KeyVaultSecret jwtKey = await client.GetSecretAsync("Jwt--Key");
            JwtKey = jwtKey.Value;

            KeyVaultSecret jwtIssuer = await client.GetSecretAsync("Jwt--Issuer");
            JwtIssuer = jwtIssuer.Value;

            KeyVaultSecret jwtAudience = await client.GetSecretAsync("Jwt--Audience");
            JwtAudience = jwtAudience.Value;

        }
    }
}
