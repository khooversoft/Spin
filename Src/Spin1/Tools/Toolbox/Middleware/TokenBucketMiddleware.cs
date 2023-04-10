using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Toolbox.Limiter;
using Toolbox.Tools;

namespace Toolbox.Middleware;

public class TokenBucketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenBucketOption _options;
    private readonly ILogger<TokenBucketMiddleware> _logger;
    private readonly IMemoryCache _memoryCache;
    private const string _key = nameof(TokenBucketMiddleware) + "_";

    public TokenBucketMiddleware(RequestDelegate next, TokenBucketOption options, IMemoryCache memoryCache, ILogger<TokenBucketMiddleware> logger)
    {
        _next = next.NotNull();
        _options = options.Verify();
        _memoryCache = memoryCache.NotNull();
        _logger = logger.NotNull();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var item in _options.ProtectPaths)
        {
            if (item.Path == "*" || context.Request.Path.StartsWithSegments(item.Path, StringComparison.OrdinalIgnoreCase))
            {
                TokenBucketPolicyState? policy = _memoryCache.GetOrCreate(ConstructKey(item.PolicyName), x => ConstructPolicyState(item));
                if (policy == null) continue;

                if (policy.Limiter.TryGetPermit())
                {
                    await _next(context);
                    return;
                }
            }
        }

        const string msg = "Too many request.";
        _logger.LogWarning(msg);
        context.Response.StatusCode = 429;
        await context.Response.WriteAsync(msg);
    }

    private string ConstructKey(string policyName) => _key + policyName.NotEmpty();

    private TokenBucketPolicyState ConstructPolicyState(TokenBucketPathOption option) => new TokenBucketPolicyState
    {
        PolicyName = option.PolicyName,
        Option = option,
        Limiter = new TokenBucketRateLimiter(option.BucketSize, option.WindowSpan),
    };
}
