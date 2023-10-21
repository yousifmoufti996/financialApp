using FinancialApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FinancialApp.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinancialApp;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "FinancialApp ",
        Description = "An ASP.NET Core Web API for managing financial " +
        "application various aspects of user accounts and financial transactions, " +
        "like registration, authentication,fund transfers between user accounts, " +
        "Retrieval of account balances, Transaction history retrieval, Account statement generation " +
        "",
        //TermsOfService = new Uri("https://example.com/terms"),
        //Contact = new OpenApiContact
        //{
        //    Name = "Example Contact",
        //    Url = new Uri("https://example.com/contact")
        //},
        //License = new OpenApiLicense
        //{
        //    Name = "Example License",
        //    Url = new Uri("https://example.com/license")
        //}
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

});

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


