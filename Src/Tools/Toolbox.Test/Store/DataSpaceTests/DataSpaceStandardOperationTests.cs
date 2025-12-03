using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.DataSpaceTests;

public class DataSpaceStandardOperationTests
{
    private ITestOutputHelper _outputHelper;

    public DataSpaceStandardOperationTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService()
    {
        var option = new DataSpaceOption
        {
            Spaces = [
                new SpaceDefinition
                {
                    Name = "file",
                    ProviderName = "fileStore",
                    BasePath = "/dataFiles",
                    SpaceFormat = SpaceFormat.Key,
                }
            ]
        };

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddInMemoryFileStore();
                //services.AddDataSpace(option);
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        //await host.ClearStore<FileStoreTransactionTests>();
        return host;
    }

    //[Fact]
    //public async Task SimpleWriteAndRead()
    //{
    //    using var host = await BuildService();
    //    var dataSpace = host.Services.GetRequiredService<DataSpace>().NotNull();
    //    var context = host.Services.CreateContext<DataSpaceStandardOperationTests>();

    //    string path = "test/data.txt";

    //    var content = "Hello, World!".ToBytes();
    //    var setResult = await dataSpace.Set(path, new DataETag(content), context);
    //    setResult.BeOk();

    //    var readOption = await dataSpace.Get(path, context);
    //    readOption.BeOk();
    //    var readData = readOption.Return().Data;
    //    content.SequenceEqual(readData).BeTrue();

    //    var s1 = await dataSpace.Search("**.*", context);
    //    s1.Count.Be(1);
    //    s1[0].Path.Be(path);

    //    s1 = await dataSpace.Search("test/*.txt", context);
    //    s1.Count.Be(1);
    //    s1[0].Path.Be(path);

    //    var deleteOption = await dataSpace.Delete(path, context);
    //    deleteOption.BeOk();

    //    var s2 = await dataSpace.Search("**.*", context);
    //    s2.Count.Be(0);
    //}

    //[Fact]
    //public async Task Standard_Operations_WorkAsExpected()
    //{
    //    using var host = await BuildService();
    //    var dataSpace = host.Services.GetRequiredService<DataSpaceFile>().NotNull();
    //    var context = host.Services.CreateContext<DataSpaceStandardOperationTests>();

    //    string path = "test/data.txt";
    //    byte[] content1 = "Hello".ToBytes();
    //    byte[] content2 = "World!".ToBytes();
    //    var setResult = await dataSpace.Set(path, new DataETag(content1), context);
    //    setResult.BeOk();

    //    var appendResult = await dataSpace.Append(path, new DataETag(content2), context);
    //    appendResult.BeOk();
    //    var getResult = await dataSpace.Get(path, context);
    //    getResult.BeOk();
    //    var finalData = getResult.Return().Data;
    //    //Assert.Equal("Hello, World!", Encoding.UTF8.GetString(finalData));
    //    var existsResult = await dataSpace.Exists(path, context);
    //    existsResult.BeOk();
    //    var deleteResult = await dataSpace.Delete(path, context);
    //    deleteResult.BeOk();
    //    var postDeleteExistsResult = await dataSpace.Exists(path, context);
    //    postDeleteExistsResult.BeOk();
    //}
}
