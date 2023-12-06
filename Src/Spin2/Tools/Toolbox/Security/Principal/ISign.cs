using Toolbox.Types;

namespace Toolbox.Security;

public interface ISign
{
    Task<Option<SignResponse>> SignDigest(string principalId, string messageDigest, string traceId);
}
