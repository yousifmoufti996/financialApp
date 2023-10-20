using FinancialApp.Utils;

namespace FinancialApp.Models.DomainModels
{
    public class Account
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }
        public string UserId { get; set; }
      
    }
}
