using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeStoreTests
{
    public readonly IDatalakeStore _dataLakeStore;
    public readonly ScopeContext _context;

    public DatalakeStoreTests(ITestOutputHelper outputHelper)
    {
        _dataLakeStore = TestApplication.GetDatalake("datastore-tests");
        _context = TestApplication.CreateScopeContext<DatalakeStoreTests>(outputHelper);
    }

    [Fact]
    public async Task GivenData_WhenSaved_ShouldWork()
    {
        const string data = "this is a test";
        const string path = "testString.txt";

        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        await _dataLakeStore.Write(path, dataBytes, true, _context);

        Option<DataETag> receive = await _dataLakeStore.Read(path, _context);
        receive.IsOk().Should().BeTrue();

        Enumerable.SequenceEqual(dataBytes, receive.Return().Data).Should().BeTrue();

        Option<DatalakePathProperties> pathProperties = await _dataLakeStore.GetPathProperties(path, _context);
        pathProperties.IsOk().Should().BeTrue();
        pathProperties.Return().ETag.Should().NotBeNull();

        (await _dataLakeStore.Exist(path, _context)).IsOk().Should().BeTrue();
        (await _dataLakeStore.GetPathProperties(path, _context)).Should().NotBeNull();

        (await _dataLakeStore.Delete(path, _context)).IsOk().Should().BeTrue();
        (await _dataLakeStore.Exist(path, _context)).IsNotFound().Should().BeTrue();

        Option<QueryResponse<DatalakePathItem>> list = await _dataLakeStore.Search(QueryParameter.Parse("**/*"), _context);
        list.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenNewFile_WhenAppended_ShouldCreateThenAppend()
    {
        const string data1 = "this is a test - first line(n)";
        const string data2 = "*** second line ****";
        const string path = "testStringAppend1.txt";

        await _dataLakeStore.Delete(path, _context);

        byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
        (await _dataLakeStore.Append(path, dataBytes, _context)).IsOk().Should().BeTrue();

        byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
        (await _dataLakeStore.Append(path, appendDataBytes, _context)).IsOk().Should().BeTrue();

        Option<DataETag> receive = await _dataLakeStore.Read(path, _context);
        receive.IsOk().Should().BeTrue();

        byte[] source = dataBytes.Concat(appendDataBytes).ToArray();
        var read = receive.Return().Data;
        source.Length.Should().Be(read.Length);

        Enumerable.SequenceEqual(source, read).Should().BeTrue();

        (await _dataLakeStore.Delete(path, _context)).IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task GivenNewFileCreated_WhenAppended_ShouldWork()
    {
        const string data1 = "this is a test - first line(n)";
        const string data2 = "*** second line ****";
        const string path = "testStringAppend1.txt";

        await _dataLakeStore.Delete(path, _context);

        byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
        (await _dataLakeStore.Write(path, dataBytes, true, _context)).IsOk().Should().BeTrue();

        Option<DataETag> readBytes = await _dataLakeStore.Read(path, _context);
        Enumerable.SequenceEqual(dataBytes, readBytes.Return().Data).Should().BeTrue();

        byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
        (await _dataLakeStore.Append(path, appendDataBytes, _context)).IsOk().Should().BeTrue();

        Option<DataETag> receive = await _dataLakeStore.Read(path, _context);
        receive.IsOk().Should().BeTrue();

        Enumerable.SequenceEqual(dataBytes.Concat(appendDataBytes), receive.Return().Data).Should().BeTrue();

        (await _dataLakeStore.Delete(path, _context)).IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task GivenExistingFile_WhenAppended_ShouldWork()
    {
        const string data = "this is a test";
        string data1 = "this is a test number 2 - first line(n)" + Environment.NewLine;
        const string data2 = "*** second line of number 2 ****";
        const string path = "testStringAppend2.txt";

        byte[] initialData = Encoding.UTF8.GetBytes(data);
        (await _dataLakeStore.Write(path, initialData, true, _context)).IsOk().Should().BeTrue();

        byte[] appendDataBytes = Encoding.UTF8.GetBytes(data1);
        (await _dataLakeStore.Append(path, appendDataBytes, _context)).IsOk().Should().BeTrue();

        byte[] append2DataBytes = Encoding.UTF8.GetBytes(data2);
        (await _dataLakeStore.Append(path, append2DataBytes, _context)).IsOk().Should().BeTrue();

        Option<DataETag> receive = await _dataLakeStore.Read(path, _context);
        receive.IsOk().Should().BeTrue();

        var full = initialData.Concat(appendDataBytes).Concat(append2DataBytes);

        Enumerable.SequenceEqual(full, receive.Return().Data).Should().BeTrue();

        (await _dataLakeStore.Delete(path, _context)).IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task GivenFiles_WhenSearched_ReturnsCorrectly()
    {
        const string fileSearchPattern = "fileSearch/**/*";
        await ClearContainer(_dataLakeStore, fileSearchPattern);

        Option<QueryResponse<DatalakePathItem>> verifyList = await _dataLakeStore.Search(QueryParameter.Parse(fileSearchPattern), _context);
        verifyList.IsOk().Should().BeTrue();
        verifyList.Return().Items.Count.Should().Be(0);

        var dataSet = new (string path, string data)[]
        {
            ("fileSearch/test1.json", "this is content for json 1"),
            ("fileSearch/test2.json", "this is content for json 2"),
            ("fileSearch/data/test3.json", "this is content for json 3"),
            ("fileSearch/data/test4.json", "this is content for json 4"),
            ("fileSearch/data2/test5.json", "this is content for json 5"),
        };

        await dataSet.ForEachAsync(async x => await _dataLakeStore.Write(x.path, x.data.ToBytes(), true, _context));

        Option<QueryResponse<DatalakePathItem>> subSearchList = await _dataLakeStore.Search(QueryParameter.Parse("fileSearch/data/**/*"), _context);
        subSearchList.IsOk().Should().BeTrue();
        subSearchList.Return().Items.Count.Should().Be(dataSet.Where(x => x.path.StartsWith("fileSearch/data/")).Count());

        Option<QueryResponse<DatalakePathItem>> searchListOption = await _dataLakeStore.Search(QueryParameter.Parse(fileSearchPattern), _context);
        searchListOption.IsOk().Should().BeTrue();
        var searchList = searchListOption.Return();
        searchList.Items.Where(x => x.IsDirectory == false).Count().Should().Be(5);
        searchList.Items.Where(x => x.IsDirectory == true).Count().Should().Be(0);

        await ClearContainer(_dataLakeStore, fileSearchPattern);

        (await _dataLakeStore.Search(QueryParameter.Parse(fileSearchPattern), _context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Items.Count.Should().Be(0);
        });
    }

    private async Task ClearContainer(IDatalakeStore dataLakeStore, string fileSearchPattern)
    {
        fileSearchPattern.NotEmpty();
        Option<QueryResponse<DatalakePathItem>> listOption = await dataLakeStore.Search(QueryParameter.Parse(fileSearchPattern), _context);
        listOption.IsOk().Should().BeTrue();
        var list = listOption.Return();

        _context.LogInformation("Delete file list={folder}", list.Items.Select(x => x.Name).Join(";"));

        foreach (var fileItem in list.Items)
        {
            _context.LogInformation("Deleting file={file}", fileItem.Name);
            (await dataLakeStore.Delete(fileItem.Name, _context)).IsOk().Should().BeTrue(fileItem.Name);
        }

        int retryCount = 5;
        int sleepSeconds = 1;

        while (retryCount-- > 0)
        {
            _context.LogInformation("Checking for files/folder");
            Option<QueryResponse<DatalakePathItem>> readListOption = await dataLakeStore.Search(QueryParameter.Parse(fileSearchPattern), _context);
            readListOption.IsOk().Should().BeTrue();
            if (readListOption.Return().Items.Count == 0)
            {
                _context.LogInformation("Verified files have been deleted");
                return;
            }

            _context.LogInformation("Files/folder still exist, retrying in {seconds} seconds", sleepSeconds);
            await Task.Delay(TimeSpan.FromSeconds(sleepSeconds));
            sleepSeconds *= 2;
        };

        _context.LogError("Failed to delete all files");
        throw new ArgumentException("Failed to delete all files");
    }
}