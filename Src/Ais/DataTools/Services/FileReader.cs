using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace DataTools.Services
{
    internal class FileReader
    {
        private readonly Counters _counters;
        private readonly ILogger<FileReader> _logger;

        public FileReader(Counters counters, ILogger<FileReader> logger)
        {
            _counters = counters;
            _logger = logger;
        }

        public IEnumerable<string> GetFiles(string file, bool recursive)
        {
            string folder = Path.GetDirectoryName(file)!;
            string search = Path.GetFileName(file);

            string[] files = Directory.GetFiles(folder, search, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            if (files.Length == 0) yield break;

            _logger.LogInformation("Getting file sizes...");

            foreach(var item in files)
            {
                var fileInfo = new FileInfo(item);
                if (fileInfo.Length == 0) continue;

                yield return item;
            }
        }

        public async Task ReadFile(string file, ITargetBlock<string> sync)
        {
            _logger.LogInformation($"Reading file {file}");
            using StreamReader stream = new StreamReader(file, Encoding.UTF8, true, 65536);

            _counters.Increment(Counter.FileRead);

            int lineCount = 0;
            while(true)
            { 
                string? line = stream.ReadLine();
                if( line == null) break;

                await sync.SendAsync(line);
                _counters.Increment(Counter.FileLine);
                lineCount++;
            }

            _logger.LogInformation($"Read file {file}, count= {lineCount:n0}");
        }
    }
}
