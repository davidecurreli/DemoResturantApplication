using Dapr.Client;

namespace Infrastructure.Helpers;

public class DaprSecretHelper
{
    public static async Task<string> RetrieveSecretAsync(string secretName)
    {
        string SECRET_STORE_NAME = "localsecretstore";
        using var client = new DaprClientBuilder().Build();
        // Using Dapr SDK to get a secret
        var secret = await client.GetSecretAsync(SECRET_STORE_NAME, secretName);
        secret.TryGetValue(secretName, out var value);

        return value ?? throw new Exception("RetrieveSecretAsync failed");
    }
}
