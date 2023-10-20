using FinancialApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FinancialApp.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinancialApp;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FinancialDBContext>(options =>
{
    options.UseInMemoryDatabase("financialdb");
});




builder.Services.AddIdentity<User, IdentityRole>(options =>
{

    options.Password.RequireLowercase = true;


})
.AddEntityFrameworkStores<FinancialDBContext>() // Replace with your actual DbContext
.AddDefaultTokenProviders();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
});


//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("UserPolicy", policy =>
//    {
//        policy.RequireRole("User"); // Users with the "User" role can access this policy.
//    });

//    options.AddPolicy("AdminPolicy", policy =>
//    {
//        policy.RequireRole("Admin"); // Users with the "Admin" role can access this policy.
//    });
//});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EncreptionClass.DecryptContent(builder.Configuration["Jwt:Key"]))) // Customize this
                    };
                });



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var dbContext = serviceProvider.GetRequiredService<FinancialDBContext>();

    UserDataSeeder.SeedUsers(userManager);

    UserDataSeeder.SeedAccounts(userManager, dbContext);
 
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


