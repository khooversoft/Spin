using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public enum SecurityAccess
{
    None = 0,
    Read = 1,
    Contributor = 2,
    Owner = 3,
}


public static class SecurityAccessTool
{
    public static Option HasAccess(this SecurityAccess subject, SecurityAccess requireAccess)
    {
        bool result = (int)subject >= (int)requireAccess;
        return result ? StatusCode.OK : StatusCode.Forbidden;
    }
}