namespace CleanArchitectureIdentityTemplate.Infrastructure.Auth
{
    public class RefreshTokenInfo
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public string Platform { get; set; }
    }
}
