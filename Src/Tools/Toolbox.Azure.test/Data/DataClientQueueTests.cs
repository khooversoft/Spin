using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Data;
using Toolbox.Test.Data.Client;
using Xunit.Abstractions;

namespace Toolbox.Azure.test;

public class DataClientQueueTests : DataQueueClientTests
{
    public DataClientQueueTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services, IDataPipelineBuilder builder)
    {
        var datalakeOption = TestApplication.ReadOption("DataClientQueueTests");
        services.AddDatalakeFileStore(datalakeOption);

        if (builder.MemoryCacheDuration != null)
            builder.MemoryCacheDuration = TimeSpan.FromMilliseconds(builder.MemoryCacheDuration.Value.TotalMilliseconds * 10);

        if (builder.FileCacheDuration != null)
            builder.FileCacheDuration = TimeSpan.FromMilliseconds(builder.FileCacheDuration.Value.TotalMilliseconds * 10);
    }
}
