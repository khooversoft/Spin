using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Toolbox.Tools;

namespace Toolbox.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _baseFileName;
        private readonly string _loggingFolder;
        private readonly int _limit;
        private readonly string _loggingFileName;
        private readonly ActionBlock<string> _output;
        private StreamWriter _logFile;

        public FileLoggerProvider(string loggingFolder, string baseLogFileName, int limit = 10)
        {
            loggingFolder.VerifyNotEmpty(nameof(loggingFolder));
            baseLogFileName.VerifyNotEmpty(nameof(baseLogFileName));
            limit.VerifyAssert(x => x > 0, "Limit must be greater then 0");

            _loggingFolder = loggingFolder;
            _baseFileName = baseLogFileName;
            _limit = limit;

            Directory.CreateDirectory(loggingFolder);
            _loggingFileName = Path.Combine(_loggingFolder, $"{_baseFileName}_{DateTime.Now.ToString("o").Replace('.', '_').Replace(':', '_')}.log");

            Directory.GetFiles(_loggingFolder, $"{_baseFileName}*.log")
                .OrderByDescending(x => x)
                .Skip(_limit)
                .ForEach(x => File.Delete(x));

            _logFile = new StreamWriter(_loggingFileName);
            _logFile.AutoFlush = true;

            _output = new ActionBlock<string>(x =>
            {
                switch (x.EndsWith(Environment.NewLine))
                {
                    case true:
                        _logFile.Write(x);
                        break;

                    default:
                        _logFile.WriteLine(x);
                        break;
                }
            });
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(_output, categoryName);

        public void Dispose()
        {
            StreamWriter streamWriter = Interlocked.Exchange(ref _logFile, null!);
            if (streamWriter != null)
            {
                _output.Complete();
                _output.Completion.Wait();

                streamWriter.Dispose();
            }
        }
    }
}

