using Microsoft.AspNetCore.Identity;
using Toolbox.Orleans;

namespace TicketShareWeb.Components.Account;
internal sealed class IdentityUserAccessor(UserManager<PrincipalIdentity> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<PrincipalIdentity> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}
