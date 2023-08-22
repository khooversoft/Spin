using Toolbox.Types;

namespace Toolbox.Security.Principal;

public interface ISign
{
    Task<Option<string>> SignDigest(string principalId, string messageDigest, string traceId);
}
