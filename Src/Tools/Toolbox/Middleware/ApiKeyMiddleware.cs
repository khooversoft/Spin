using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Tools;

namespace Toolbox.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKeyName;
        private readonly string _apiKey;
        private readonly string[] _bypassPaths;

        public ApiKeyMiddleware(RequestDelegate next, string apiKeyName, string apiKey, string[] bypassPaths)
        {
            next.VerifyNotNull(nameof(next));
            apiKeyName.VerifyNotNull(nameof(apiKeyName));
            apiKey.VerifyNotNull(nameof(apiKey));
            bypassPaths.VerifyNotNull(nameof(bypassPaths));

            _next = next;
            _apiKeyName = apiKeyName;
            _apiKey = apiKey;
            _bypassPaths = bypassPaths;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_bypassPaths.Any(x => context.Request.Path.StartsWithSegments(x, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(_apiKeyName, out StringValues extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync($"{_apiKeyName} was not provided.");
                return;
            }

            if (_apiKey != extractedApiKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}
