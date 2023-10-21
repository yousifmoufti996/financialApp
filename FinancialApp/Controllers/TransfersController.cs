using FinancialApp.Data;
using FinancialApp.Models.DataTransferObject;
using FinancialApp.Models.DomainModels;
using FinancialApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace FinancialApp.Controllers
{
    
    
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]

    [ApiController]
    public class TransfersController : ControllerBase
    {
        private readonly FinancialDBContext _dbContext;
        private readonly UserManager<User> userManager;


        public TransfersController(FinancialDBContext dbContext ,UserManager<User> userManager)
        {
            _dbContext = dbContext;
            this.userManager = userManager;
        }


        ///// <summary>
        ///// This endpoint requires authentication.
        ///// </summary>
        ///// <remarks>
        ///// To access this endpoint, include a valid JWT token in the 'Authorization' header as a bearer token.
        ///// Example: 'Authorization: Bearer your-token-here'
        ///// </remarks>
        //[HttpGet("GetAlltransactions")]
        //public IActionResult GetAlltransactions()
        //{

        //    //}

        //    var claimsIdentity = this.User.Identity as ClaimsIdentity;

        //    var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
        //    if (userId == null)
        //    {
        //        return Unauthorized("User not authenticated.");
        //    }
        //    var trans = _dbContext.Transactions.Where(note => note.Date != null).ToList();

        //    var noteslist = trans.Select(note => new Transaction
        //    {
        //        Id = note.Id,
        //        Amount = note.Amount,
        //            Type = note.Type,
        //            Date = note.Date,
        //            SourceAccountId = note.SourceAccountId,
        //            DestinationAccountId = note.DestinationAccountId,
        //    }).ToList();


        //    return Ok(noteslist);
        //}

        /// <summary>
        ///  This endpoint requires authentication.
        /// </summary>
        /// <remarks>
        /// A post methode that transfer money from the user who authenticated to another user.
        /// To access this endpoint, include a valid JWT token in the 'Authorization' header as a bearer token.
        /// Example: 'Authorization: Bearer your-token-here'
        /// </remarks>

        /// <response code="200">Returns 
        /// {
        ///  "success": true,
        ///  "message": "Funds transferred successfully.you balance is: 900.0"
        ///}
        /// </response>

        /// <response code="401">If Invalid Token or the password that you provided is not for the user</response>
        /// <response code="400">If Insufficient funds in the source accountor  their is mistake in the body If this user does not have account or the distenation account is your account (you cant transfer to yourself)</response>
        /// <response code="500">If An error occurred it shows the message</response>
        /// <response code="404">If this user does not have an account or the url is wrong or destination account not found</response>
        [HttpPost("FundTransfer")]
        public async Task<IActionResult> FundTransfer(TransferRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input. Please provide transfer details.");
            }
            var userId = "";
            var account = new Account();
            var user = new User();

            try
            {
                 userId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
           
                try
                {

                  user = await userManager.FindByIdAsync(userId);
                }
                catch (Exception ex) {
                    return Unauthorized("Invalid Token.");
                }

                
               
                
                //throw new Exception("sdsdasdsdas");
                var result = await userManager.CheckPasswordAsync(user, request.Password); // Use userManager
                if (!result)
                {
                    return Unauthorized("Invalid password.");
                }

                account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
                if (account == null)
                {
                    return NotFound("Account not found.");
                }
              

                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex}");
            }

            // Retrieve source and destination accounts
            var sourceAccount = account.Id.ToString();


            var destinationAccount = _dbContext.Accounts.SingleOrDefault(a => a.Id == Guid.Parse( request.DestinationAccountId));

            if (sourceAccount == null || destinationAccount == null)
            {
                return NotFound("One or more accounts not found.");
            }
            else if(request.DestinationAccountId == sourceAccount)
            {
                return BadRequest("you cant transfer to yourself.");
            }



            else if (account.Balance < request.Amount)
            {
                return BadRequest("Insufficient funds in the source account.");
            }


            try
            {
                // Create transaction records

                // Update account balances
                var transaction = new Transaction
                {
                    Amount = request.Amount,
                    Type = TransactionType.Transfer,
                    Date = DateTime.Now,
                    BalanceBeforeTransaction = account.Balance,
                    SourceAccountId = account.Id.ToString() ,
                    DestinationAccountId = destinationAccount.Id.ToString(),
                    Id = new Guid()
                };
                account.Balance -= request.Amount;
                destinationAccount.Balance += request.Amount;

                // Create transaction records (if needed)
                _dbContext.Transactions.Add(transaction);
                _dbContext.SaveChanges();

                return Ok(new TransferResponse { Success = true, Message = "Funds transferred successfully."+ "you balance is: " + account.Balance });
            }
            catch (Exception ex)
            {
                // Handle exceptions here, and log the exception for debugging
                // You can return a specific HTTP status code or a custom error message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }






        /// <summary>
        ///  This endpoint requires authentication.
        /// </summary>
        /// <remarks>
        /// A GET methode that Retrieve Account Balance of the user who authenticated.
        /// To access this endpoint, include a valid JWT token in the 'Authorization' header as a bearer token.
        /// Example: 'Authorization: Bearer your-token-here'
        /// </remarks>
        /// <response code="200">Returns 
        /// {
        ///   "balance": 5000.0
        ///}
        /// </response>
        /// <response code="401">If Invalid Token </response>
        /// <response code="500">If An error occurred it shows the message</response>
        /// <response code="404">If this user doesn't have an account or the url is wrong </response>
        [HttpGet("RetrievalAccountBalance")]
        public async Task<IActionResult> GetAccountBalance()
        {
            var userId = "";
            var account = new Account();

            try
            {
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized("Invalid Token.");
                }
            

                account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);

                if (account == null)
                {
                    return NotFound("Account not found.");
                }



            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

            return Ok(new { Balance = account.Balance });
        }




        /// <summary>
        /// This endpoint requires authentication.
        /// </summary>
        /// <remarks>
        ///  A GET methode that's return the TransactionHistory of an authenticated user.
        /// To access this endpoint, include a valid JWT token in the 'Authorization' header as a bearer token.
        /// Example: 'Authorization: Bearer your-token-here'
        /// </remarks>
        /// <response code="401">If Invalid Token</response>
        /// <response code="400">If this user does not have account</response>
        /// <response code="500">If An error occurred it shows the message</response>
        [HttpGet("TransactionHistoryRetrieval")]
        public async Task<IActionResult> GetTransactionHistory()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized("Invalid user ID.");
                }

                var account = _dbContext.Accounts.FirstOrDefault(a => a.UserId == userId);
                var accountId = new Guid();

                if (account != null)
                {
                    // Account found, and you can access its Id
                     accountId = account.Id;
                    // Use accountId as needed
                }
                else
                {
                    return BadRequest("this user does not have account.");
                }


                var transactions = await _dbContext.Transactions
                    .Where(t => t.SourceAccountId == accountId.ToString() || t.DestinationAccountId == accountId.ToString())
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                var transactionHistory = transactions.Select(t => new
                {
                    Date = t.Date,
                    Amount = t.Amount,
                    Source = t.SourceAccountId,
                    Destination = t.DestinationAccountId
                });

                return Ok(transactionHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }







        /// <summary>
        ///This endpoint requires authentication.
        /// </summary>
        /// <remarks>
        ///  A GET methode that Generates Account Statement for the user who authenticated.
        /// To access this endpoint, include a valid JWT token in the 'Authorization' header as a bearer token.
        /// Example: 'Authorization: Bearer your-token-here'
        /// </remarks>

       
        /// <response code="200">Returns 
        /// {
        /// "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        /// "startDate": "2023-10-21T01:21:55",
        /// "endDate": "2023-11-21T00:00:00Z",
        /// "startingBalance": 550.0,
        /// "endingBalance": 500.0,
        /// "transactions": [
        ///         {
        ///         "id": "f628eed1-83b8-47b8-b998-780bac9802d2",
        ///     "amount": 50,
        ///             "balanceBeforeTransaction": 550.0,
        ///         "type": 2,
        ///     "date": "2023-10-21T01:22:06.2595494+03:00",
        ///             "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "destinationAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa7"
        /// }
        /// ]
        ///  }
        /// </response>

        /// <response code="401">If Invalid Token </response>
        /// <response code="400">If The startDate is before the account CreatedDate OR their is mistake in the body If this user does not have account</response>
        /// <response code="500">If An error occurred it shows the message</response>
        /// <response code="404">If this user does not have an account or the url is wrong or destination account not found</response>
        [HttpGet("GenerateAccountStatement")]
        public IActionResult GenerateAccountStatement(DateTime startDate, DateTime endDate)
        {
            try
            {


                // Retrieve the user's account and transactions within the date range
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;



                var userAccount = _dbContext.Accounts.FirstOrDefault(a => a.UserId == userId);

                if (userAccount == null)
                {
                    return NotFound("Account not found.");
                }


                if (userAccount.CreatedDate > startDate)
                {
                    return BadRequest("The startDate is before the account CreatedDate");
                }


                var transactions = _dbContext.Transactions
                    .Where(t => (t.SourceAccountId == userAccount.Id.ToString() || t.DestinationAccountId == userAccount.Id.ToString()) && t.Date >= startDate && t.Date <= endDate)
                    .OrderBy(t => t.Date)
                    .ToList();
                decimal StartingBalance = 0;
                if (transactions.Count != 0)
                {
                    StartingBalance = transactions[0].BalanceBeforeTransaction;
                    //return NotFound("Account not found.");
                }

                // Calculate the account balance over the statement period


                decimal endingBalance = StartingBalance;

                foreach (var transaction in transactions)
                {
                    if (transaction.Type == TransactionType.Deposit)
                    {
                        endingBalance += transaction.Amount;
                    }
                    else if (transaction.Type == TransactionType.Withdrawal)
                    {
                        endingBalance -= transaction.Amount;
                    }
                    else if (transaction.Type == TransactionType.Transfer)
                    {
                        // For transfers, consider both source and destination accounts
                        if (transaction.SourceAccountId == userAccount.Id.ToString())
                        {
                            endingBalance -= transaction.Amount;
                        }
                        else if (transaction.DestinationAccountId == userAccount.Id.ToString())
                        {
                            endingBalance += transaction.Amount;
                        }
                    }
                }

                // Generate a report or PDF (example: JSON response)
                var statement = new
                {
                    AccountId = userAccount.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    StartingBalance = StartingBalance,
                    EndingBalance = endingBalance,
                    Transactions = transactions,
                };

                return Ok(statement);

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");

            }
        }

    }
}
