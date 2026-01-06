using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Test.Store.KeyStore;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class Datalake_TypedTests : KeyStore_TypedTests
{
    public Datalake_TypedTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    protected override void AddStore(IServiceCollection services, string basePath)
    {
        var option = TestApplication.ReadOption(basePath);
        services.AddDatalakeFileStore(option);
    }
}
