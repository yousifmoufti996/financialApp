using FinancialApp.Models.DomainModels;
using FinancialApp.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinancialApp.Data
{
    public class FinancialDBContext : DbContext
    {

        public FinancialDBContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnConfiguring
            (DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase(databaseName: "financialdb").AddInterceptors(new SoftDeleteInterceptor());
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property<bool>("IsDeleted");
            modelBuilder.Entity<User>().Property<DateTimeOffset?>("DeletedAt");

            modelBuilder.Entity<User>().HasQueryFilter(user => !user.IsDeleted);


            



        }


        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Statement> Statements { get; set; }
    }
}
