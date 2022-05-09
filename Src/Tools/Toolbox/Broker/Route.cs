using System;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Broker
{
    public record Route<T> : IRoute
    {
        public Route(string pattern, Func<T, Task> receiver)
        {
            pattern.NotEmpty(nameof(pattern));
            receiver.NotNull(nameof(receiver));

            Pattern = pattern;
            Receiver = receiver;
        }

        public string Pattern { get; }

        public Func<T, Task> Receiver { get; }

        public async Task SendToReceiver(object subject) => await Receiver((T)subject);
    }
}