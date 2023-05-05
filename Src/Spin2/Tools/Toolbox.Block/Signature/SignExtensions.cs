﻿using Toolbox.Block.Container;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Security.Sign;
using Toolbox.Tools;

namespace Toolbox.Block.Signature;

public static class SignExtensions
{
    public static IReadOnlyList<PrincipleDigest> GetPrincipleDigests(this BlockChain blockChain, bool onlyUnsighed = true)
    {
        blockChain.NotNull();

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

        var signedDigests = principleDigests.Sign(getPrincipleSignature);
        return blockChain.Sign(signedDigests);
    }

    public static IReadOnlyList<PrincipleDigest> Sign(this IEnumerable<PrincipleDigest> principleDigests, Func<string, IPrincipalSignature> getPrincipleSignature)
    {
        List<PrincipleDigest> signedDigests = principleDigests
            .Select(x => x with { JwtSignature = getPrincipleSignature(x.PrincipleId).Sign(x.Digest) })
            .ToList();

        return signedDigests;
    }

    public static BlockChain Sign(this BlockChain blockChain, IEnumerable<PrincipleDigest> principleDigests)
    {
        blockChain.NotNull();
        principleDigests.NotNull();

        return new BlockChain(blockChain.Blocks.Select(x =>
        {
            if (x.DataBlock.JwtSignature.IsNotEmpty()) return x;

            PrincipleDigest principleDigest = principleDigests
                .Where(y => y.PrincipleId == x.DataBlock.PrincipleId && y.Digest == x.DataBlock.Digest)
                .FirstOrDefault()
                .NotNull(name: $"Cannot locate principleId={x.DataBlock.PrincipleId} and digest={x.DataBlock.Digest} in {nameof(principleDigests)}")
                .Assert(x => !x.JwtSignature.IsEmpty(), x => $"{nameof(x.JwtSignature)} is required");

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
            item.JwtSignature.NotEmpty(name: $"JwtSignature is required, key={item.Key}");

            string kid = JwtTokenParser.GetKidFromJwtToken(item.JwtSignature)
                .NotEmpty(name: "JWT kid not found");

            IPrincipalSignature principleSignature = getPrincipleSignature(kid)
                .NotNull(name: "No principle signature returned");

            principleSignature.ValidateSignature(item.Digest);
        }
    }
}