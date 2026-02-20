namespace CleanArchitectureIdentityTemplate.Application.DTOs
{
    public class TwoFactorDto
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string Code { get; set; }
    }
}
