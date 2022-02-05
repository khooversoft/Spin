using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Security.Sign;
using Toolbox.Tools;

namespace Toolbox.Block;

public static class SignExtensions
{
    public static IReadOnlyList<PrincipleDigest> GetPrincipleDigests(this BlockChain blockChain, bool onlyUnsighed = true)
    {
        blockChain.VerifyNotNull(nameof(blockChain));

        return blockChain.Blocks
            .Where(x => !onlyUnsighed || x.DataBlock.JwtSignature.IsEmpty())
            .Select(x => new PrincipleDigest
            {
                Key = x.DataBlock.BlockId,
                PrincipleId = x.DataBlock.PrincipleId,
                Digest = x.DataBlock.Digest,
                JwtSignature = x.DataBlock.JwtSignature,
            }).ToList();
    }

    public static BlockChain Sign(this BlockChain blockChain, Func<string, IPrincipalSignature> getPrincipleSignature)
    {
        IReadOnlyList<PrincipleDigest> principleDigests = blockChain.GetPrincipleDigests();

        List<PrincipleDigest> signedDigests = principleDigests
            .Select(x => x with { JwtSignature = getPrincipleSignature(x.PrincipleId).Sign(x.Digest) })
            .ToList();

        return blockChain.Sign(signedDigests);
    }

    public static BlockChain Sign(this BlockChain blockChain, IEnumerable<PrincipleDigest> principleDigests)
    {
        blockChain.VerifyNotNull(nameof(blockChain));
        principleDigests.VerifyNotNull(nameof(principleDigests));

        return new BlockChain(blockChain.Blocks.Select(x =>
        {
            if (!x.DataBlock.JwtSignature.IsEmpty()) return x;

            PrincipleDigest principleDigest = principleDigests
                .Where(y => y.PrincipleId == x.DataBlock.PrincipleId && y.Digest == x.DataBlock.Digest)
                .FirstOrDefault()
                .VerifyNotNull($"Cannot locate principleId={x.DataBlock.PrincipleId} and digest={x.DataBlock.Digest} in {nameof(principleDigests)}")
                .VerifyAssert(x => !x.JwtSignature.IsEmpty(), x => $"{nameof(x.JwtSignature)} is required");

            return x with { DataBlock = x.DataBlock with { JwtSignature = principleDigest.JwtSignature! } };
        }));
    }

    public static void Validate(this BlockChain blockChain, Func<string, IPrincipalSignature> getPrincipleSignature)
    {
        IReadOnlyList<PrincipleDigest> principleDigests = blockChain.GetPrincipleDigests();
        principleDigests.Validate(getPrincipleSignature);
    }

    public static void Validate(this IEnumerable<PrincipleDigest> principleDigests, Func<string, IPrincipalSignature> getPrincipleSignature)
    {
        foreach (var item in principleDigests)
        {
            item.JwtSignature.VerifyNotEmpty($"JwtSignature is required, key={item.Key}");

            string kid = JwtTokenParser.GetKidFromJwtToken(item.JwtSignature)
                .VerifyNotEmpty("JWT kid not found");

            IPrincipalSignature principleSignature = getPrincipleSignature(kid)
                .VerifyNotNull($"No principle signature returned");

            principleSignature.ValidateSignature(item.Digest);
        }
    }
}
