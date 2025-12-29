//using System.Text;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.Store;

//public class FileStoreFileAccessStandardTests
//{
//    private readonly IFileStore _fileStore;
//    public readonly ScopeContext _context;

//    public FileStoreFileAccessStandardTests(IFileStore fileStore, ScopeContext context) => (_fileStore, _context) = (fileStore.NotNull(), context);

//    public async Task GivenData_WhenSaved_ShouldWork()
//    {
//        const string data = "this is a test";
//        const string path = "testString.txt";

//        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
//        var fileClient = _fileStore.File(path);

//        await fileClient.Set(dataBytes, _context);

//        (await _fileStore.Search(path, _context)).Count.Be(1);

//        Option<DataETag> receive = await fileClient.Get(_context);
//        receive.IsOk().BeTrue();

//        Enumerable.SequenceEqual(dataBytes, receive.Return().Data).BeTrue();

//        (await fileClient.Exists(_context)).IsOk().BeTrue();

//        (await fileClient.GetDetails(_context)).Action(x =>
//        {
//            x.IsOk().BeTrue();
//            x.Return().ETag.NotNull();
//        });

//        (await fileClient.GetDetails(_context)).IsOk().BeTrue();

//        (await fileClient.Delete(_context)).IsOk().BeTrue();
//        (await fileClient.Exists(_context)).IsNotFound().BeTrue();

//        (await _fileStore.Search(path, _context)).Count.Be(0);
//    }

//    public async Task GivenNewFile_WhenAppended_ShouldCreateThenAppend()
//    {
//        const string data1 = "this is a test - first line(n)";
//        const string data2 = "*** second line ****";
//        const string path = "testStringAppend2.txt";

//        var fileClient = _fileStore.File(path);

//        await fileClient.Delete(_context);

//        byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
//        (await fileClient.Append(dataBytes, _context)).IsOk().BeTrue();

//        byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
//        (await fileClient.Append(appendDataBytes, _context)).IsOk().BeTrue();

//        Option<DataETag> receive = await fileClient.Get(_context);
//        receive.IsOk().BeTrue();

//        byte[] source = dataBytes.Concat(appendDataBytes).ToArray();
//        var read = receive.Return().Data;
//        source.Length.Be(read.Length);

//        Enumerable.SequenceEqual(source, read).BeTrue();

//        (await fileClient.Delete(_context)).IsOk().BeTrue();
//    }

//    public async Task GivenNewFileCreated_WhenAppended_ShouldWork()
//    {
//        const string data1 = "this is a test - first line(n)";
//        const string data2 = "*** second line ****";
//        const string path = "testStringAppend3.txt";

//        var fileClient = _fileStore.File(path);

//        await fileClient.Delete(_context);

//        byte[] dataBytes = Encoding.UTF8.GetBytes(data1);
//        (await fileClient.Set(dataBytes, _context)).IsOk().BeTrue();

//        Option<DataETag> readBytes = await fileClient.Get(_context);
//        Enumerable.SequenceEqual(dataBytes, readBytes.Return().Data).BeTrue();

//        byte[] appendDataBytes = Encoding.UTF8.GetBytes(data2);
//        (await fileClient.Append(appendDataBytes, _context)).IsOk().BeTrue();

//        Option<DataETag> receive = await fileClient.Get(_context);
//        receive.IsOk().BeTrue();

//        Enumerable.SequenceEqual(dataBytes.Concat(appendDataBytes), receive.Return().Data).BeTrue();

//        (await fileClient.Get(_context)).IsOk().BeTrue();
//    }

//    public async Task GivenExistingFile_WhenAppended_ShouldWork()
//    {
//        const string data = "this is a test";
//        string data1 = "this is a test number 2 - first line(n)" + Environment.NewLine;
//        const string data2 = "*** second line of number 2 ****";
//        const string path = "testStringAppend2.txt";

//        var fileClient = _fileStore.File(path);

//        byte[] initialData = Encoding.UTF8.GetBytes(data);
//        (await fileClient.Set(initialData, _context)).IsOk().BeTrue();

//        byte[] appendDataBytes = Encoding.UTF8.GetBytes(data1);
//        (await fileClient.Append(appendDataBytes, _context)).IsOk().BeTrue();

//        byte[] append2DataBytes = Encoding.UTF8.GetBytes(data2);
//        (await fileClient.Append(append2DataBytes, _context)).IsOk().BeTrue();

//        Option<DataETag> receive = await fileClient.Get(_context);
//        receive.IsOk().BeTrue();

//        var full = initialData.Concat(appendDataBytes).Concat(append2DataBytes).ToArray();

//        var receiveBytes = receive.Return().Data;
//        Enumerable.SequenceEqual(full, receiveBytes).BeTrue();
//    }

//    public async Task GivenFiles_WhenSearched_ReturnsCorrectly()
//    {
//        const string fileSearchPattern = "fileSearch/**/*";
//        await ClearContainer(fileSearchPattern);

//        IReadOnlyList<StorePathDetail> verifyList = await _fileStore.Search(fileSearchPattern, _context);
//        verifyList.Count.Be(0);

//        var dataSet = new (string path, string data)[]
//        {
//            ("fileSearch/test1.json", "this is content for json 1"),
//            ("fileSearch/test2.json", "this is content for json 2"),
//            ("fileSearch/data/test3.json", "this is content for json 3"),
//            ("fileSearch/data/test4.json", "this is content for json 4"),
//            ("fileSearch/data2/test5.json", "this is content for json 5"),
//        };

//        await dataSet.ForEachAsync(async x => await _fileStore.File(x.path).Set(x.data.ToBytes(), _context));

//        IReadOnlyList<StorePathDetail> subSearchList = await _fileStore.Search("fileSearch/data/**/*", _context);
//        subSearchList.Count.Be(dataSet.Where(x => x.path.StartsWith("fileSearch/data/")).Count());

//        IReadOnlyList<StorePathDetail> searchList = await _fileStore.Search(fileSearchPattern, _context);
//        searchList.Where(x => x.IsFolder == false).Count().Be(5);
//        searchList.Where(x => x.IsFolder == true).Count().Be(0);

//        await ClearContainer(fileSearchPattern);

//        (await _fileStore.Search(fileSearchPattern, _context)).Count.Be(0);
//    }

//    private async Task ClearContainer(string fileSearchPattern)
//    {
//        fileSearchPattern.NotEmpty();

//        IReadOnlyList<StorePathDetail> list = await _fileStore.Search(fileSearchPattern, _context);

//        _context.LogInformation("Delete file list={folder}", list.Select(x => x.Path).Join(";"));

//        foreach (var fileItem in list)
//        {
//            _context.LogInformation("Deleting file={file}", fileItem.Path);
//            (await _fileStore.File(fileItem.Path).Delete(_context)).IsOk().BeTrue(fileItem.Path);
//        }
//    }
//}
