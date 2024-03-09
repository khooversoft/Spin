using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Toolbox.Tools;
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
