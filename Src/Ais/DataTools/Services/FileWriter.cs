using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace DataTools.Services
{
    internal class FileWriter : IDisposable
    {
        private readonly ILogger _logger;
        private StreamWriter? _writeStream;
        private readonly string _fileName;
        private readonly IReadOnlyList<string> _headers;
        private int _fileCount = 0;
        private const int _maxLineCount = 10_000_000;
        private int _LineCount = 0;

        public FileWriter(string fileName, IEnumerable<string> headers, ILogger logger)
        {
            _fileName = fileName.VerifyNotEmpty(nameof(fileName));
            _logger = logger.VerifyNotNull(nameof(logger));

            _headers = headers
                .VerifyNotNull(nameof(headers))
                .ToList();
        }

        public void Write(string line)
        {
            line.VerifyNotEmpty(nameof(line));

            _writeStream ??= CreateFile();
            _writeStream.WriteLine(line);

            _LineCount++;
            if (_LineCount >= _maxLineCount)
            {
                Close();
                _LineCount = 0;
            }
        }

        public void Close()
        {
            StreamWriter? streamWriter = Interlocked.Exchange(ref _writeStream, null);
            streamWriter?.Close();
        }

        private StreamWriter CreateFile()
        {
            string? folder = Path.GetDirectoryName(_fileName);
            string file = Path.GetFileNameWithoutExtension(_fileName).VerifyNotEmpty("No file name");
            string extension = Path.GetExtension(_fileName) ?? ".tsv";

            string outFile = Path.Combine(folder ?? string.Empty, $"{file}_{_fileCount++}{extension}");
            _logger.LogInformation($"Writing to file {outFile}");

            StreamWriter fileStream = new StreamWriter(outFile);

            string header = new StringVector("\t").AddRange(_headers);
            fileStream.WriteLine(header);

            return fileStream;
        }

        public void Dispose() => Close();
    }
}
