using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Test.Store.ListStore;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.KeyStoreFromTest;

public class ListSpace_StressTests : SpaceListStressTests
{
    public ListSpace_StressTests(ITestOutputHelper output) : base(output) { }

    protected override void AddStore(IServiceCollection services, string basePath)
    {
        var datalakeOption = TestApplication.ReadOption(basePath);
        services.AddDatalakeFileStore(datalakeOption);
    }
}
