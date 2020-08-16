using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Toolbox.Tools
{
    public class TimerAsync
    {
        private readonly TimeSpan _frequency;
        private readonly Func<string, CancellationToken, Task> _func;
        private readonly ILogger<TimerAsync> _logger;
        private readonly string _workId = nameof(TimerAsync) + "." + Guid.NewGuid().ToString();
        private readonly ActionBlock<CancellationToken> _actionBlock;
        private Timer? _timer;

        public TimerAsync(TimeSpan frequency, Func<string, CancellationToken, Task> func, ILogger<TimerAsync> logger, string? workId = null)
        {
            func.VerifyNotNull(nameof(func));
            logger.VerifyNotNull(nameof(logger));

            _workId = workId.ToNullIfEmpty() ?? nameof(TimerAsync) + "." + Guid.NewGuid().ToString();

            _frequency = frequency;
            _func = func;
            _logger = logger;

            _actionBlock = new ActionBlock<CancellationToken>(DoWork);
        }

        public TimerAsync Start(CancellationToken token)
        {
            Interlocked.CompareExchange(
                ref _timer,
                new Timer(_ => _actionBlock.Post(token), null, _frequency, _frequency),
                null);

            return this;
        }

        public void Stop() => Interlocked.Exchange(ref _timer, null)?.Dispose();

        private async Task DoWork(CancellationToken token)
        {
            _logger.LogTrace($"{nameof(DoWork)}: Executing");
            await _func(_workId, token);
        }
    }
}
