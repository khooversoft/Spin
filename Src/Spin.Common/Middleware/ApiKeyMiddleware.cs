using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Tools;

namespace Spin.Common.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKeyName;
        private readonly string _apiKey;
        private readonly IReadOnlyList<string> _bypassPaths;

        public ApiKeyMiddleware(RequestDelegate next, string apiKeyName, string apiKey, IEnumerable<string> bypassPaths)
        {
            next.NotNull();
            apiKeyName.NotNull();
            apiKey.NotNull();
            bypassPaths.NotNull();

            _next = next;
            _apiKeyName = apiKeyName;
            _apiKey = apiKey;
            _bypassPaths = bypassPaths.ToList();
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
