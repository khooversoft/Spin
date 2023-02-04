using System;
using System.Collections.Generic;
using Toolbox.Block.Application;

namespace Spin.Common.Sign;

public record SignRequestResponse
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public IReadOnlyList<PrincipleDigest> PrincipleDigests { get; init; } = new List<PrincipleDigest>();

    public IReadOnlyList<string>? Errors { get; init; }
}
