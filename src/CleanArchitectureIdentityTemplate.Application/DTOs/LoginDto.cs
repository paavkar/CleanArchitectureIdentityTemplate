using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureIdentityTemplate.Application.DTOs
{
    public class LoginDto
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }

        [Required]
        public string Password { get; set; } = "";
    }
}
