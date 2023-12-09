using Toolbox.Types;

namespace Toolbox.Security;

public interface ISignValidate
{
    Task<Option> ValidateDigest(string jwtSignature, string messageDigest, string traceId);
}