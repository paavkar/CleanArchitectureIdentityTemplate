namespace CleanArchitectureIdentityTemplate.Application.ResultModels
{
    public class AuthResult : BaseResult
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool TwoFactorRequired { get; set; }
        public string? TwoFactorUri { get; set; }
    }
}
