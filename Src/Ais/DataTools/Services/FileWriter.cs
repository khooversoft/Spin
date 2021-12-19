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
    internal class FileWriter : IDisposable
    {
        private readonly ILogger _logger;
        private StreamWriter? _writeStream;
        private readonly string _fileName;
        private readonly string _headers;
        private readonly string _batchDate;
        private int _fileCount = 0;
        private const int _maxLineCount = 10_000_000;
        private int _LineCount = 0;

        public FileWriter(string fileName, string batchDate, IEnumerable<string> headers, ILogger logger)
        {
            _fileName = fileName.VerifyNotEmpty(nameof(fileName));
            _batchDate = batchDate.VerifyNotEmpty(nameof(batchDate));
            _logger = logger.VerifyNotNull(nameof(logger));

            _headers = headers
                .VerifyNotNull(nameof(headers))
                .Join("\t");
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

            string outFile = Path.Combine(folder ?? string.Empty, $"{file}_{_batchDate}_{_fileCount++}{extension}");
            _logger.LogInformation($"Writing to file {outFile}");

            StreamWriter fileStream = new StreamWriter(outFile, false, Encoding.UTF8, 65536);

            fileStream.WriteLine(_headers);

            return fileStream;
        }

        public void Dispose() => Close();
    }
}
