using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeStoreTests
{
    public readonly IDatalakeStore _dataLakeStore;
    public readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public DatalakeStoreTests()
    {
        _dataLakeStore = TestApplication.GetDatalake("datastore-tests");
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

        Option<QueryResponse<DatalakePathItem>> list = await _dataLakeStore.Search(QueryParameter.Default, _context);
        list.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenNewFile_WhenAppended_ShouldWork()
    {
        const string data1 = "this is a test - first line(n)";
        const string data2 = "*** second line ****";
        const string path = "testStringAppend1.txt";

        byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
        (await _dataLakeStore.Write(path, dataBytes, true, _context)).IsOk().Should().BeTrue();

        Option<DataETag> readBytes = await _dataLakeStore.Read(path, _context);
        Enumerable.SequenceEqual(dataBytes, readBytes.Return().Data).Should().BeTrue();

        byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
        (await _dataLakeStore.Append(path, appendDataBytes, _context)).Should().Be(StatusCode.OK);

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
        await ClearContainer(_dataLakeStore);

        Option<QueryResponse<DatalakePathItem>> verifyList = await _dataLakeStore.Search(QueryParameter.Default, _context);
        verifyList.IsOk().Should().BeTrue();
        verifyList.Return().Items.Count.Should().Be(0);

        var dataSet = new (string path, string data)[]
        {
            ("test1.json", "this is content for json 1"),
            ("test2.json", "this is content for json 2"),
            ("data/test3.json", "this is content for json 3"),
            ("data/test4.json", "this is content for json 4"),
            ("data2/test5.json", "this is content for json 5"),
        };

        await dataSet
            .ForEachAsync(async x => await _dataLakeStore.Write(x.path, x.data.ToBytes(), true, _context));

        Option<QueryResponse<DatalakePathItem>> subSearchList = await _dataLakeStore.Search(new QueryParameter { Filter = "data/**/*" }, _context);
        subSearchList.IsOk().Should().BeTrue();
        subSearchList.Return().Items.Count.Should().Be(dataSet.Where(x => x.path.StartsWith("data/")).Count());

        Option<QueryResponse<DatalakePathItem>> searchList = await _dataLakeStore.Search(QueryParameter.Default, _context);
        searchList.IsOk().Should().BeTrue();
        searchList.Return().Items.Where(x => x.IsDirectory == false).Count().Should().Be(2);
        searchList.Return().Items.Where(x => x.IsDirectory == true).Count().Should().Be(2);

        await ClearContainer(_dataLakeStore);

        searchList = await _dataLakeStore.Search(QueryParameter.Default, _context);
        searchList.IsOk().Should().BeTrue();
        searchList.Return().Items.Count.Should().Be(0);
    }

    private async Task ClearContainer(IDatalakeStore dataLakeStore)
    {
        Option<QueryResponse<DatalakePathItem>> list = await dataLakeStore.Search(QueryParameter.Default, _context);
        list.IsOk().Should().BeTrue();

        foreach (var fileItem in list.Return().Items.Where(x => x.IsDirectory == true))
        {
            (await dataLakeStore.DeleteDirectory(fileItem.Name, _context)).IsOk().Should().BeTrue();
        }

        foreach (var fileItem in list.Return().Items.Where(x => x.IsDirectory == false))
        {
            (await dataLakeStore.Delete(fileItem.Name, _context)).IsOk().Should().BeTrue();
        }
    }
}
