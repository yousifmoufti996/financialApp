namespace FinancialApp.Models.DomainModels
{
    public class LoginDto
    {
    
        public string Email { get; set; }
        public string Password { get; set; }


    }

    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        
        public string? FullName { get; set; }
        public int Age { get; set; }
        public string? Address { get; set; }
    }
}