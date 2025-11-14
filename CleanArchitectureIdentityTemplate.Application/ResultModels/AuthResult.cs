namespace CleanArchitectureIdentityTemplate.Application.ResultModels
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
