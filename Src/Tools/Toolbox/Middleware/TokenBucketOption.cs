using System;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Middleware;

public record TokenBucketOption
{
    public IReadOnlyList<TokenBucketPathOption> ProtectPaths { get; init; } = Array.Empty<TokenBucketPathOption>();
}

public record TokenBucketPathOption
{
    public string PolicyName { get; init; } = null!;
    public string Path { get; init; } = null!;
    public int BucketSize { get; init; } = 10;
    public TimeSpan WindowSpan { get; init; } = TimeSpan.FromSeconds(10);
}


public static class TokenBucketOptionExtensions
{
    public static TokenBucketOption Verify(this TokenBucketOption subject)
    {
        subject.NotNull();
        subject.ProtectPaths.NotNull();
        subject.ProtectPaths.ForEach(x => x.Verify());

        return subject;
    }

    public static TokenBucketPathOption Verify(this TokenBucketPathOption subject)
    {
        subject.NotNull();
        subject.PolicyName.NotEmpty();
        subject.Path.NotEmpty();
        subject.BucketSize.Assert(x => x > 0, "Bucket size");

        return subject;
    }
}
