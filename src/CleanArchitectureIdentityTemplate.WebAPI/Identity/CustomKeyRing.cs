using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureIdentityTemplate.WebAPI.Identity
{
    public class CustomKeyRing(IConfiguration configuration) : ILookupProtectorKeyRing
    {
        public string this[string keyId] => keyId;
        public string CurrentKeyId => configuration.GetSection("EncryptionKeys")
                .Get<EncryptionKeys>() is var k ? k.Current : null;
        public IEnumerable<string> GetAllKeyIds() => [.. configuration.GetSection("EncryptionKeys")
                                                     .Get<EncryptionKeys>()
                                                     .Values
                                                     .Keys];
    }
}
