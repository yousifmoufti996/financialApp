using FinancialApp.Data;
using FinancialApp.Models.DomainModels;
using FinancialApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace FinancialApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    /// <summary>
    /// This is a sample API controller.
    /// </summary>
    public class AuthController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly FinancialDBContext dbContext;
        private readonly UserManager<User> userManager;
     

        public AuthController(IConfiguration configuration, FinancialDBContext dbContext, UserManager<User> userManager)
        {
            _configuration = configuration;
            this.dbContext = dbContext;
            this.userManager = userManager;

        }






        /// <remarks>
        ///  A POST methode for creating a new user (registreation proccess) and creates an account for this user.
        /// </remarks>


        /// <response code="200">Returns 
        ///         {
        ///         "result": {
        ///       "succeeded": true,
        ///   "errors": []
        ///             },
        ///       "account": {
        ///     "id": "c6052046-d614-4edb-8f67-334270cda3ce",
        /// "balance": 5000,
        ///             "userId": "fbcedad5-119a-40f8-8d0c-40b05e31b829",
        ///         "createdDate": "0001-01-01T00:00:00"
        ///   }
        /// }
        /// </response>

        /// <response code="400">Email and password are required OR their is mistake in the input </response>

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {

            

            // Ensure that the model contains the necessary properties, including 'Password'
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Invalid input. Please provide email and password.");
            }

            // Check if the username is already in use
            if (dbContext.Users.Any(u => u.Email == model.Email))
            {
                return Conflict("Email already in use.");
            }

            // Hash the password before storing it in the database
            //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                UserName = model.Username,
             
                Email = model.Email,
                Address  = model.Address,

            };

            try
            {
                
                var result = await userManager.CreateAsync(user, model.Password);

          

                if (result.Succeeded)
                {
                    
                    // If user creation is successful, create an associated account
                    var account = new Account
                    {
                        Balance = 5000.0m, // You can set the initial balance here
                        UserId = user.Id // Associate the account with the user
                    };

                    // Add the account to the database
                    dbContext.Accounts.Add(account);
                    dbContext.SaveChanges();

                    return Ok(new { result, account } );
                }
                else
                {
                    // Handle any registration errors
                    return BadRequest("User registration failed. Please check your input. Email and password are required");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
               
            }



        }



















        /// <remarks>
        ///  A POST methode for logging in and returning a token.
        /// </remarks>


        /// <response code="200">Returns 
        ///  {"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiZmJjZWRhZDUtMTE5YS00MGY4LThkMGMtNDBiMDVlMzFiODI5IiwibmJmIjoxNjk3ODUxNTc3LCJleHAiOjE2OTc4NTUxNzcsImlhdCI6MTY5Nzg1MTU3NywiaXNzIjoiaXNzdWVyIiwiYXVkIjoiYXVkaWVuY2UifQ.qdK8rLbDdV8TPkQ_mLiI4rH1BiIfTRETSddztaHV7X4"}
    /// </response>
    /// <response code="400"> if not providding both email and password  OR their is mistake in the input </response>
    /// <response code="401"> if Invalid Email or password   OR their is mistake in the input </response>
    [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            if (login == null || string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Invalid input. Pleaseeeee provide both email and password.");
            }

            //var _user = dbContext.Users.FirstOrDefault(x => x.UserName == login.Username && x.Password == login.Password);

            var user = await userManager.FindByEmailAsync(login.Email); // Use userManager

            if (user == null)
            {
                return Unauthorized("Invalid Email.");
            }

            var result = await userManager.CheckPasswordAsync(user, login.Password); // Use userManager
            //var result = BCrypt.Net.BCrypt.Verify(login.Password, user);

            if (result)
            {
                var token = GenerateToken(user, EncreptionClass.DecryptContent(_configuration["Jwt:Key"]));
                return Ok(new { token });
            }
            else
            {
                return Unauthorized("Invalid Email or password.");
            }
        }






        private string GenerateToken(User user, string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.Id)
        }),

                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],


            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }















    }










}
