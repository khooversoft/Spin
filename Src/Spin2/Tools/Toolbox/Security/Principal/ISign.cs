using Toolbox.Types;

namespace Toolbox.Security.Principal;

public interface ISign
{
    Task<Option<string>> SignDigest(string kid, string messageDigest, string traceId);
}
