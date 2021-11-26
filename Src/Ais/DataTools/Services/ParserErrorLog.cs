using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DataTools.Services
{
    internal class ParserErrorLog : IDisposable
    {
        private readonly ILogger<ParserErrorLog> _logger;
        private readonly AisStore _aisStore;
        private StreamWriter? _writer;

        public ParserErrorLog(AisStore aisStore, ILogger<ParserErrorLog> logger)
        {
            _aisStore = aisStore.VerifyNotNull(nameof(aisStore));
            _logger = logger.VerifyNotNull(nameof(logger));
        }

        public void Dispose()
        {
            StreamWriter? current = Interlocked.Exchange(ref _writer, null);
            current?.Close();
        }

        public void LogParseError(string line, Exception ex)
        {
            _writer ??= OpenWriter();

            var json = new
            {
                Line = line,
                Ex = ex.ToString(),
            }.ToJson();

            _writer.WriteLine(json);
        }

        private StreamWriter OpenWriter()
        {
            string file = Path.Combine(_aisStore.StoreFolder, $"ErrorLog_{_aisStore.BatchDate}.log");
            _logger.LogInformation($"Writing to parser error file : {file}");

            return new StreamWriter(file);
        }
    }
}
