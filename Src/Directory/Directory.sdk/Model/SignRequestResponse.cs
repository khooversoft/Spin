using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Block;
using Toolbox.Tools;

namespace Directory.sdk.Model;

public record SignRequestResponse
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public IReadOnlyList<PrincipleDigest> PrincipleDigests { get; init; } = new List<PrincipleDigest>();

    public IReadOnlyList<string>? Errors { get; init; }
}
