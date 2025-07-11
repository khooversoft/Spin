using Microsoft.AspNetCore.Identity;
using Toolbox.Types;

namespace Toolbox.Identity;

public static class IdentityResultTool
{
    public static IdentityResult ToIdentityResult(this Option subject)
    {
        var identityResult = subject switch
        {
            { StatusCode: StatusCode.OK } => IdentityResult.Success,
            _ => IdentityResult.Failed(new IdentityError { Code = "Conflict", Description = subject.Error ?? "< no error >" }),
        };

        return identityResult;
    }
}
