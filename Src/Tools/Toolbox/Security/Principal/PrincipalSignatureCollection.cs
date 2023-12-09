using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Security;

public class PrincipalSignatureCollection : ISign, ISignValidate
{
    private ConcurrentDictionary<string, IPrincipalSignature> _principalList = new ConcurrentDictionary<string, IPrincipalSignature>();

    public PrincipalSignatureCollection() { }
    public PrincipalSignatureCollection(IEnumerable<IPrincipalSignature> signers) => signers.NotNull().ForEach(x => _principalList[x.Kid] = x);

    public PrincipalSignatureCollection Add(params IPrincipalSignature[] signers) => this.Action(_ => signers.NotNull().ForEach(x => _principalList[x.Kid] = x));

    public void Clear() => _principalList.Clear();
    public bool Remove(string kid) => _principalList.TryRemove(kid, out var _);

    public async Task<Option<SignResponse>> SignDigest(string kid, string messageDigest, string traceId)
    {
        if (!_principalList.TryGetValue(kid, out var principalSignature)) return new Option<SignResponse>(StatusCode.NotFound, "kid not found");

        var result = await principalSignature.SignDigest(kid, messageDigest, traceId);
        return result;
    }

    public async Task<Option> ValidateDigest(string jwtSignature, string messageDigest, string traceId)
    {
        string? kid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        if (kid == null) return new Option(StatusCode.BadRequest, "no kid in jwtSignature");

        if (!_principalList.TryGetValue(kid, out var principalSignature)) return new Option(StatusCode.NotFound, "kid not found");

        var result = await principalSignature.ValidateDigest(jwtSignature, messageDigest, traceId);
        return result;
    }
}
