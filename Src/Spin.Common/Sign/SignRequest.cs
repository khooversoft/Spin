using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Abstractions.Tools;
using Toolbox.Block.Application;

namespace Spin.Common.Sign;

public record SignRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public IReadOnlyList<PrincipleDigest> PrincipleDigests { get; init; } = new List<PrincipleDigest>();
}


public static class SignRequestExtensions
{
    public static void Verify(this SignRequest subject)
    {
        subject.NotNull();
        subject.PrincipleDigests.NotNull();
        subject.PrincipleDigests.Assert(x => x.Count > 0, nameof(subject.PrincipleDigests));
    }

    public static SignRequest ToSignRequest(this IEnumerable<PrincipleDigest> digests) => new SignRequest { PrincipleDigests = digests.ToList() };
}
