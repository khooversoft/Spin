using Toolbox.Security.Jwt;
using Toolbox.Types;

namespace Toolbox.Security.Principal;

public interface ISignValidate
{
    Task<Option> ValidateDigest(string jwtSignature, string messageDigest, ScopeContext context);
    //Task<Option<JwtTokenDetails>> ValidateDigest(string jwtSignature, string messageDigest, ScopeContext context);
}