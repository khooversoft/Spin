using Microsoft.Extensions.Logging;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DataTools.Services
{
    internal class ParserErrorLog : IAsyncDisposable
    {
        private readonly ILogger<ParserErrorLog> _logger;
        private readonly AisStore _aisStore;
        private StreamWriter? _writer;
        private ActionBlock<(string line, Exception ex)> _queue;

        public ParserErrorLog(AisStore aisStore, ILogger<ParserErrorLog> logger)
        {
            _aisStore = aisStore.VerifyNotNull(nameof(aisStore));
            _logger = logger.VerifyNotNull(nameof(logger));

            _queue = new ActionBlock<(string line, Exception ex)>(
                x => InternalLogParseError(x.line, x.ex),
                new ExecutionDataflowBlockOptions { BoundedCapacity = 1000 }
                );
        }

        public void LogParseError(string line, Exception ex) => _queue.Post((line, ex));

        public void InternalLogParseError(string line, Exception ex)
        {
            _writer ??= OpenWriter();
            if (_writer == null) return;

            var json = new
            {
                Line = line,
                Ex = ex.ToString(),
            }.ToJson();

            _writer.WriteLine(json);
        }

        private StreamWriter? OpenWriter()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    string file = Path.Combine(_aisStore.StoreFolder, $"ErrorLog_{_aisStore.BatchDate}_{i}.log");

                    var stream = new StreamWriter(file);
                    _logger.LogInformation($"Writing to parser error file : {file}");
                    return stream;
                }
                catch { }
            }

            return null;
        }

        public async ValueTask DisposeAsync()
        {
            _queue.Complete();
            await _queue.Completion;

            StreamWriter? current = Interlocked.Exchange(ref _writer, null);
            current?.Close();
        }
    }
}
