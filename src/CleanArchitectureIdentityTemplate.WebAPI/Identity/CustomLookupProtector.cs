using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace CleanArchitectureIdentityTemplate.WebAPI.Identity
{
    public class CustomLookupProtector(IConfiguration configuration) : ILookupProtector
    {
        public string Protect(string keyId, string? data)
        {
            if (string.IsNullOrWhiteSpace(data)) return string.Empty;


            using Aes aes = Aes.Create();

            var secret = configuration[$"EncryptionKeys:Values:{keyId}"]
                     ?? throw new Exception($"Key {keyId} not found!");

            aes.Key = Encoding.UTF8.GetBytes(secret.PadRight(32)[..32]);
            // For deterministic encryption, we use a fixed IV or an IV derived from the data
            aes.IV = new byte[16];

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            var inputBytes = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Unprotect(string keyId, string? data)
        {
            if (string.IsNullOrWhiteSpace(data)) return string.Empty;

            using Aes aes = Aes.Create();

            var secret = configuration[$"EncryptionKeys:Values:{keyId}"]
                     ?? throw new Exception($"Key {keyId} not found!");

            aes.Key = Encoding.UTF8.GetBytes(secret.PadRight(32)[..32]);
            aes.IV = new byte[16];

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            var inputBytes = Convert.FromBase64String(data);
            var decryptedBytes = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
