﻿using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Block;
using Toolbox.Tools;

namespace Directory.sdk.Model;

public record SignRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public IReadOnlyList<PrincipleDigest> PrincipleDigests { get; init; } = new List<PrincipleDigest>();
}


public static class SignRequestExtensions
{
    public static void Verify(this SignRequest subject)
    {
        subject.NotNull(nameof(subject));
        subject.PrincipleDigests.NotNull(nameof(subject.PrincipleDigests));
        subject.PrincipleDigests.Assert(x => x.Count > 0, nameof(subject.PrincipleDigests));
    }

    public static SignRequest ToSignRequest(this IEnumerable<PrincipleDigest> digests) => new SignRequest { PrincipleDigests = digests.ToList() };
}
