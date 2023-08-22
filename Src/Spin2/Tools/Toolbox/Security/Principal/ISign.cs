using Toolbox.Security.Sign;
using Toolbox.Types;

namespace Toolbox.Security.Principal;

public interface ISign
{
    Task<Option<SignResponse>> SignDigest(string principalId, string messageDigest, string traceId);
}
