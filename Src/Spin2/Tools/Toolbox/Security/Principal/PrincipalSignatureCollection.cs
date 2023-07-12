using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Security.Principal;

public class PrincipalSignatureCollection : ISign, ISignValidate
{
    private ConcurrentDictionary<string, IPrincipalSignature> _principalList = new ConcurrentDictionary<string, IPrincipalSignature>();

    public PrincipalSignatureCollection() { }
    public PrincipalSignatureCollection(IEnumerable<IPrincipalSignature> signers) => signers.NotNull().ForEach(x => _principalList[x.Kid] = x);

    public PrincipalSignatureCollection Add(params IPrincipalSignature[] signers) => this.Action(_ => signers.NotNull().ForEach(x => _principalList[x.Kid] = x));

    public void Clear() => _principalList.Clear();
    public bool Remove(string kid) => _principalList.TryRemove(kid, out var _);

    public async Task<Option<string>> SignDigest(string kid, string messageDigest, ScopeContext context)
    {
        if (!_principalList.TryGetValue(kid, out var principalSignature)) return new Option<string>(StatusCode.NotFound, "kid not found");

        var result = await principalSignature.SignDigest(kid, messageDigest, context);
        return result;
    }

    public async Task<Option<JwtTokenDetails>> ValidateDigest(string jwtSignature, string messageDigest, ScopeContext context)
    {
        string? kid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        if (kid.IsEmpty()) return new Option<JwtTokenDetails>(StatusCode.BadRequest, "jwt token does not have kid").LogResult(context.Location());

        if (!_principalList.TryGetValue(kid, out var principalSignature)) return new Option<JwtTokenDetails>(StatusCode.NotFound, "kid not found").LogResult(context.Location());

        var result = await principalSignature.ValidateDigest(jwtSignature, messageDigest, context);
        return result;
    }
}
