using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureIdentityTemplate.Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = "";
    }
}
