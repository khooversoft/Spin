using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Test.Store.KeyStore;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Store;

public class Space_CacheTests : DataSpaceCacheTests
{
    public Space_CacheTests(ITestOutputHelper output) : base(output) { }

    protected override void AddStore(IServiceCollection services, string basePath)
    {
        var datalakeOption = TestApplication.ReadOption(basePath);
        services.AddDatalakeFileStore(datalakeOption);
    }
}