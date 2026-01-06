using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Test.Store.KeyStore;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class Datalake_LeaseTests : KeyStore_LeaseTests
{
    public Datalake_LeaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    protected override void AddStore(IServiceCollection services, string basePath)
    {
        var option = TestApplication.ReadOption(basePath);
        services.AddDatalakeFileStore(option);
    }
}
