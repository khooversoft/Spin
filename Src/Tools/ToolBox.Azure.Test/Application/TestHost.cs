using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions.Extensions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Azure.Queue;

namespace ToolBox.Azure.Test.Application;

internal class TestHost
{
    private const string _configFile = @"d:\SpinDisk\\Environments\test-azure.json";
    private IReadOnlyList<DirectoryEntry>? _entries;
    private ILoggerFactory? _loggerFactory;

    private TestHost() { }

    public static TestHost Default { get; } = new TestHost();

    public IReadOnlyList<DirectoryEntry> Entries => _entries ??= new DirectoryEntryBuilder().Add(_configFile).Build();

    public ILoggerFactory GetLoggerFactory() => _loggerFactory ??= LoggerFactory.Create(builder => builder.AddDebug());

    public ILogger<T> CreateLogger<T>() => GetLoggerFactory().CreateLogger<T>();

    public DatalakeStoreOption GetDatalakeStoreOption() => Entries
        .First(x => x.DirectoryId == "test-storage")
        .ConvertTo<DatalakeStoreOption>()
        .Verify();

    public QueueOption GetQueueOption() => Entries
        .First(x => x.DirectoryId == "test-queue")
        .ConvertTo<QueueOption>()
        .Verify();

    public static string WriteResourceToFile(string fileName)
    {
        string filePath = Path.Combine(Path.GetTempPath(), nameof(TestHost), fileName);
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        string testData = Enumerable.Range(0, 20)
            .Select(x => $"{x} test data line")
            .Join(Environment.NewLine);

        using Stream stream = new MemoryStream(testData.ToBytes());
        using Stream writeFile = new FileStream(filePath, FileMode.Create);
        stream.CopyTo(writeFile);

        return filePath;
    }
}
