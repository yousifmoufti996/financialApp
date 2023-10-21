using System.ComponentModel.DataAnnotations;

namespace FinancialApp.Models.DomainModels
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }


    }

    public class RegisterDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string? Username { get; set; }
        
        public string? FullName { get; set; }
        public int? Age { get; set; }

        public string? Address { get; set; }
    }
}