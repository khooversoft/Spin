using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Pattern;
using Toolbox.Tools;

namespace Toolbox.Broker
{
    public class Router
    {
        private readonly ConcurrentDictionary<string, IRoute> _routes = new(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<Router> _logger;
        private PatternSelect? _patternSelect;

        public Router(ILogger<Router> logger)
        {
            _logger = logger;
        }

        public Func<object, Task>? Forward { get; set; }

        public async Task Send(string path, object message)
        {
            path.NotEmpty(nameof(path));
            message.NotNull(nameof(message));

            _patternSelect ??= new PatternSelect()
                .Action(x => _routes.Values.ForEach(y => x.AddPattern(y.Pattern, y.Pattern)));

            (bool matched, PatternResult? result) = _patternSelect.TryMatch<PatternResult>(path);

            if (!matched || result == null)
            {
                _logger.LogTrace($"Message path='{path}'does not match routes for message='{message}', forwarded");
                await ForwardMessage(message);
                return;
            }

            if( !_routes.TryGetValue(result.Pattern, out IRoute? route))
            {
                _logger.LogError($"Pattern='{result.Pattern}' not registered");
                return;
            }

            try
            {
                _logger.LogTrace($"Sending message for path={path}");
                await route.SendToReceiver(message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Send messaged failed, name={result.Name}, pattern={result.Pattern}");
                throw;
            }
        }

        public Router Add(params IRoute[] routes)
        {
            routes.ForEach(y =>
            {
                _routes[y.Pattern] = y;
                _logger.LogTrace($"{Compiler.Location}: adding route {y}");
            });

            _patternSelect = null;
            return this;
        }

        public Router Delete(string path)
        {
            _routes.TryRemove(path.NotEmpty(nameof(path)), out IRoute? _);
            _patternSelect = null;
            return this;
        }

        public Router SetForward(Func<object, Task> forward) => this.Action(x => x.Forward = forward);

        private async Task ForwardMessage(object message)
        {
            if (Forward != null) await Forward(message);
        }
    }
}