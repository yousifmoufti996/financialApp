namespace FinancialApp.Models.DomainModels
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime Date { get; set; }

        public string SourceAccountId { get; set; }
        public string DestinationAccountId { get; set; }

        //public Guid Id { get; set; }
        //public decimal Amount { get; set; }
        //public TransactionType Type { get; set; }
        //public DateTime Date { get; set; }


        //public Account Account { get; set; }
        //public Guid SourceAccountId { get; set; } 
        //public Account SourceAccount { get; set; }
        //public Guid DestinationAccountId { get; set; } 
        //public Account DestinationAccount { get; set; }
    }
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer,
    }
}
