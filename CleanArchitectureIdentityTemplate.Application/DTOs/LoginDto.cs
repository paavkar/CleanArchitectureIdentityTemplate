using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureIdentityTemplate.Application.DTOs
{
    public class LoginDto
    {
        [Required]
        public string EmailOrUsername { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
