using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace ApplicationGateway.Helpers;

public class RsaSecurityHelper
{
    public static RsaSecurityKey CreateRsaSecurityKey()
    {
        var pemX509 = GetPublicKey();
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(pemX509);

        return new RsaSecurityKey(rsa);
    }
    public static string GetPublicKey()
    {
        var key = File.ReadAllText(Path.GetFullPath("./Certificate/publicKey.pem"));

        return key.Replace(Environment.NewLine, string.Empty);
    }
}
