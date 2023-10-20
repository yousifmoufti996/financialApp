using Microsoft.AspNetCore.Identity;
//using notesApp.API.Models.DomainModels;

namespace FinancialApp.Utils;

public class User : IdentityUser
{

    public int Age { get; set; }
    public string Address { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }


}
