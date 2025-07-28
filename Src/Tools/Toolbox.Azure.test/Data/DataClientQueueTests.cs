using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Test.Data.Client;
using Xunit.Abstractions;

namespace Toolbox.Azure.test;

public class DataClientQueueTests : DataQueueClientTests
{
    public DataClientQueueTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("datastore-hybridCache-tests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
