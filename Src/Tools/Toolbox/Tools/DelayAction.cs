using System;
using System.Threading;

namespace Toolbox.Tools
{
    public class DelayAction : IDisposable
    {
        private readonly TimeSpan _timeSpan;
        private Action? _action;
        private Timer _timer;

        public DelayAction(TimeSpan timeSpan)
        {
            _timer = new Timer(Send);
            _timeSpan = timeSpan;
        }

        public void Dispose() => Interlocked.Exchange(ref _timer, null!)?.Dispose();

        public void Post(Action action)
        {
            _timer.Change(_timeSpan, TimeSpan.FromMilliseconds(-1));
            _action = action;
        }

        private void Send(object? obj)
        {
            try
            {
                Interlocked.Exchange(ref _action, null)?.Invoke();
            }
            finally
            {
                _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
            }
        }
    }
}