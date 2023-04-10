using Toolbox.Limiter;

namespace Toolbox.Middleware;

public record TokenBucketPolicyState
{
    public string PolicyName { get; init; } = null!;
    public TokenBucketPathOption Option { get; init; } = null!;
    public TokenBucketRateLimiter Limiter { get; init; } = null!;
}
