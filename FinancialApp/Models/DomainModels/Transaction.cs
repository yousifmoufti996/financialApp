namespace FinancialApp.Models.DomainModels
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBeforeTransaction { get; set; }
        public TransactionType Type { get; set; }
        public DateTime Date { get; set; }

        public string SourceAccountId { get; set; }
        public string DestinationAccountId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }



    }
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer,
    }
}
