namespace FinancialApp.Models.DomainModels
{
    public class Statement
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public decimal StartingBalance { get; set; }
        public decimal EndingBalance { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
