using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;
using ToolBox.Azure.Test.Application;
using Xunit;
using Xunit.Abstractions;

namespace ToolBox.Azure.Test.DataLake
{
    public class DatalakePerformanceTests
    {
        private readonly DataLakeStoreOption _testOption;
        private readonly ILoggerFactory _loggerFactory = new TestLoggerFactory();
        private readonly ITestOutputHelper _output;
        private int _totalCount;

        public DatalakePerformanceTests(ITestOutputHelper output)
        {
            _testOption = new TestOptionBuilder().Build() with { ContainerName = "adls-performance" };

            _output = output;
        }

        [Fact]
        public async Task GivenTelemetryStreamModel_ForMultiplePartitions_ShouldPerform()
        {
            await InitializeFileSystem();

            const int max = 10;
            CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            _totalCount = 0;

            Stopwatch sw = Stopwatch.StartNew();

            Task[] tasks = Enumerable.Range(0, max)
                .Select(x => RunPartition(x, tokenSource.Token))
                .ToArray();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) { }

            sw.Stop();

            _output.WriteLine($"Total count={_totalCount}, MS={sw.ElapsedMilliseconds}, Sec={sw.Elapsed.TotalSeconds}, TPS / {_totalCount / sw.Elapsed.TotalSeconds}");
        }

        private async Task InitializeFileSystem()
        {
            IDataLakeFileSystem management = new DataLakeFileSystem(_testOption, _loggerFactory.CreateLogger<DataLakeFileSystem>());
            await management.CreateIfNotExist(_testOption.ContainerName, CancellationToken.None);

            IDataLakeStore dataLakeStore = new DataLakeStore(_testOption, _loggerFactory.CreateLogger<DataLakeStore>());
            await ClearContainer(dataLakeStore);
        }

        private async Task RunPartition(int partition, CancellationToken token)
        {
            DateTime timestamp = DateTime.Now;

            IDataLakeStore dataLakeStore = new DataLakeStore(_testOption, _loggerFactory.CreateLogger<DataLakeStore>()); ;

            int count = 0;
            while (!token.IsCancellationRequested)
            {
                var data = new
                {
                    Partition = partition,
                    Timestamp = DateTime.Now,
                    Type = "trace",
                    RecordId = count,
                    Data = $"This is {count} record id"
                };
                count++;

                if (count % 100 == 0)
                {
                    timestamp = timestamp + TimeSpan.FromDays(1);
                }

                string json = Json.Default.Serialize(data) + Environment.NewLine;
                string path = $"telemetry/data/{timestamp.Year:D4}/{timestamp.Month:D2}/{timestamp.Day:D2}/{partition:D6}/trace.json";

                await dataLakeStore.Append(path, Encoding.UTF8.GetBytes(json), token);

                Interlocked.Increment(ref _totalCount);
            }
        }

        private async Task ClearContainer(IDataLakeStore dataLakeStore)
        {
            IReadOnlyList<DataLakePathItem> list = await dataLakeStore.Search(null, x => true, false, CancellationToken.None);
            list.Should().NotBeNull();

            foreach (var fileItem in list.Where(x => x.IsDirectory == true))
            {
                await dataLakeStore.DeleteDirectory(fileItem.Name!, CancellationToken.None);
            }

            foreach (var fileItem in list.Where(x => x.IsDirectory == false))
            {
                await dataLakeStore.Delete(fileItem.Name!, CancellationToken.None);
            }
        }
    }
}