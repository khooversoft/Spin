using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Toolbox.Tools.Local
{
    public class MonitorStream
    {
        private readonly ActionBlock<(int id, string dataLine)> _stream;
        private int _messageBlockId = 0;

        public MonitorStream(Func<string, Task> monitor)
        {
            monitor.VerifyNotNull(nameof(monitor));

            _stream = new ActionBlock<(int id, string dataLine)>(async x =>
            {
                if (x.id != _messageBlockId) return;
                await monitor(x.dataLine);
            });
        }

        public void Post(string dataLine) => _stream.Post((_messageBlockId, dataLine));

        public void NewMessageBlockId() => Interlocked.Increment(ref _messageBlockId);
    }
}
