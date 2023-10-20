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


        [HttpGet("GetAlltransactions")]
        
        public IActionResult GetAlltransactions()
        {

            //string userId = GetUserIdFromToken();
            //if (userId == null)
            //{
            //    return Unauthorized();

            //}

            var claimsIdentity = this.User.Identity as ClaimsIdentity;

            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }
            var trans = _dbContext.Transactions.Where(note => note.Date != null).ToList();

            var noteslist = trans.Select(note => new Transaction
            {
                Id = note.Id,
                Amount = note.Amount,
                    Type = note.Type,
                    Date = note.Date,
                    SourceAccountId = note.SourceAccountId,
                    DestinationAccountId = note.DestinationAccountId,
            }).ToList();


            return Ok(noteslist);
        }







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
                //if (!Guid.TryParse(userId, out Guid userGuid))
                //{
                //    return BadRequest("Invalid user ID format.");
                //}
                try
                {

                  user = await userManager.FindByIdAsync(userId);
                }
                catch (Exception ex) {
                    return Unauthorized("Invalid Token.");
                }

                if (string.IsNullOrWhiteSpace(userId) )
                {
                    return Unauthorized("Invalid Token.");
                }
                if (string.IsNullOrEmpty(request.Password))
                {
                    return Unauthorized("please enter your password.");
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
                var transaction = new Transaction
                {
                    Amount = request.Amount,
                    Type = TransactionType.Transfer,
                    Date = DateTime.Now,
                    SourceAccountId = account.Id.ToString() ,
                    DestinationAccountId = destinationAccount.Id.ToString(),
                    Id = new Guid()
                };

                // Update account balances
                account.Balance -= request.Amount;
                destinationAccount.Balance += request.Amount;

                // Create transaction records (if needed)
                _dbContext.Transactions.Add(transaction);
                _dbContext.SaveChanges();

                return Ok(new TransferResponse { Success = true, Message = "Funds transferred successfully." });
            }
            catch (Exception ex)
            {
                // Handle exceptions here, and log the exception for debugging
                // You can return a specific HTTP status code or a custom error message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }





        [HttpGet("RetrievalAccountBalances")]
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
                //if (!Guid.TryParse(userId, out Guid userGuid))
                //{
                //    return BadRequest("Invalid user ID format.");
                //}

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






        [HttpGet("TransactionHistoryRetrieval ")]
        public async Task<IActionResult> GetTransactionHistory()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized("Invalid user ID.");
                }

                //if (!Guid.TryParse(userId, out Guid userGuid))
                //{
                //    return BadRequest("Invalid user ID format.");
                //}


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
                    return BadRequest("this user dose not have account.");
                }


                //if (!Guid.TryParse(userId, out Guid userGuid))
                //{
                //    return BadRequest("Invalid user ID format.");
                //}
                //Debug.WriteLine($"userId: {userId}, userGuid: {userGuid}");

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

        [HttpGet("GenerateAccountStatement")]
        public IActionResult GenerateAccountStatement(DateTime startDate, DateTime endDate)
        {
            // Retrieve the user's account and transactions within the date range
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            //if (!Guid.TryParse(userId, out Guid userGuid))
            //{
            //    return BadRequest("Invalid user ID format.");
            //}

            var userAccount = _dbContext.Accounts.FirstOrDefault(a => a.UserId == userId);

            if (userAccount == null)
            {
                return NotFound("Account not found.");
            }
            
          
           


            var transactions = _dbContext.Transactions
                .Where(t => (t.SourceAccountId == userAccount.Id.ToString()|| t.DestinationAccountId == userAccount.Id.ToString()) && t.Date >= startDate && t.Date <= endDate)
                .OrderBy(t => t.Date)
                .ToList();

            // Calculate the account balance over the statement period
            decimal currentBalance = userAccount.Balance;
            foreach (var transaction in transactions)
            {
                currentBalance += transaction.Amount;
            }

            // Generate a report or PDF (example: JSON response)
            var statement = new
            {
                AccountId = userAccount.Id,
                StartDate = startDate,
                EndDate = endDate,
                StartingBalance = userAccount.Balance,
                EndingBalance = currentBalance,
                Transactions = transactions,
            };

            return Ok(statement);
        }













        //private string CheckUserAccount()
        //{


        //    var userId = "";
        //    var account = new Account();

        //    try
        //    {
        //        userId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        //        if (string.IsNullOrWhiteSpace(userId))
        //        {
        //            //     return Unauthorized("Invalid Token.");
        //        }
        //        if (!Guid.TryParse(userId, out Guid userGuid))
        //        {
        //            return BadRequest("Invalid user ID format.");
        //        }

        //        account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == userGuid);

        //        if (account == null)
        //        {
        //            return NotFound("Account not found.");
        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}




    }
}
