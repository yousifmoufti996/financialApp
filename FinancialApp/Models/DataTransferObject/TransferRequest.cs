using System.ComponentModel.DataAnnotations;

namespace FinancialApp.Models.DataTransferObject
{
    public class TransferRequest
    {
        [Required]

        public string DestinationAccountId { get; set; }
        [Required]

        public string Password { get; set; }
        [Required]


        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
    }

    // TransferResponse model for the response data
    public class TransferResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
