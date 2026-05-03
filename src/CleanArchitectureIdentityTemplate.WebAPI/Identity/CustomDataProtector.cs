using Microsoft.AspNetCore.Identity;

namespace CleanArchitectureIdentityTemplate.WebAPI.Identity
{
    public class CustomDataProtector : IPersonalDataProtector
    {
        private readonly ILookupProtector _lookupProtector;
        private readonly ILookupProtectorKeyRing _keyRing;

        public CustomDataProtector(ILookupProtector lookupProtector, ILookupProtectorKeyRing keyRing)
        {
            _lookupProtector = lookupProtector;
            _keyRing = keyRing;
        }

        public string Protect(string data)
        {
            return $"{_keyRing.CurrentKeyId}:{_lookupProtector.Protect(_keyRing.CurrentKeyId, data)}";
        }

        public string Unprotect(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            var index = data.IndexOf(':');

            return index == -1
                ? _lookupProtector.Unprotect(_keyRing.CurrentKeyId, data)
                : _lookupProtector.Unprotect(data.Substring(0, index), data.Substring(index + 1));
        }
    }
}
