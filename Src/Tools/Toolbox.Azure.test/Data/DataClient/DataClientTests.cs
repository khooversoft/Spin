using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Data;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data.DataClient;

public class DataClientTests : Test.Data.Client.DataClientTests
{
    public DataClientTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services, IDataPipelineBuilder builder)
    {
        var datalakeOption = TestApplication.ReadOption("DataClientTests");
        services.AddDatalakeFileStore(datalakeOption);

        if (builder.MemoryCacheDuration != null)
            builder.MemoryCacheDuration = TimeSpan.FromMilliseconds(builder.MemoryCacheDuration.Value.TotalMilliseconds * 10);

        if (builder.FileCacheDuration != null)
            builder.FileCacheDuration = TimeSpan.FromMilliseconds(builder.FileCacheDuration.Value.TotalMilliseconds * 10);
    }
}
