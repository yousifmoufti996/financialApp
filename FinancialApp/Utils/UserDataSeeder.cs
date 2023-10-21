using FinancialApp.Data;
using FinancialApp.Models.DomainModels;
using Microsoft.AspNetCore.Identity;

namespace FinancialApp.Utils
{
    public class UserDataSeeder
    {
        public static void SeedAccounts(UserManager<User> userManager, FinancialDBContext dbContext)
        {
            var user1 = userManager.FindByNameAsync("user1").Result;
            var user2 = userManager.FindByNameAsync("user2").Result;

            if (user1 != null)
            {
                var account1 = new Account
                {
                    Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), //Guid.NewGuid(), // Generate a unique ID for the account
                    Balance = 1000.0m, // Set the initial balance
                    UserId = user1.Id,
                    CreatedDate = DateTime.Now
                };
                dbContext.Accounts.Add(account1);
            }

            if (user2 != null)
            {
                var account2 = new Account
                {
                    Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa7"),//Guid.NewGuid(), // Generate a unique ID for the account
                    Balance = 1500.0m, // Set the initial balance
                    UserId = user2.Id,
                    CreatedDate = DateTime.Now

                };
                dbContext.Accounts.Add(account2);
            }

            dbContext.SaveChanges();
        }



        public static void SeedUsers(UserManager<User> userManager)
        {
            if (userManager.FindByNameAsync("user1").Result == null)
            {
                User user1 = new User
                {
                    UserName = "user1",
                    Email = "user1@example.com",
                    Age = 30,
                    Address = "Address 1"
                    // Add any other user properties here
                };

    
                IdentityResult result = userManager.CreateAsync(user1, "Password@123").Result;

            }

            if (userManager.FindByNameAsync("user2").Result == null)
            {
                User user2 = new User
                {
                    UserName = "user2",
                    Email = "user2@example.com",
                    Age = 25,
                    Address = "Address 2"
                    // Add any other user properties here
                };

                IdentityResult result = userManager.CreateAsync(user2, "Password@123").Result;

              
            }
        }
    }

}
