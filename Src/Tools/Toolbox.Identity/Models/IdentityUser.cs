//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNetCore.Identity;

//namespace Toolbox.Identity;

//public record IdentityUserx : IdentityUser, IUser<string>
//{
//    public IdentityUserx()
//    {
//    }

//    public string Id { get; init; } = null!;
//    public string UserName { get; set; } = null!;
//    public string? PasswordHash { get; init; }
//    public string? SecurityStamp { get; init; }
//    //    Claims
//    //    Logins
//    //    Roles

//    public IdentityRole Role { get; init; }
//    public IdentityRoleClaim<string> RoleClaim { get; init; }
//}
