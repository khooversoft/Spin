using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Toolbox.Test.Store;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Store;

public class KeyStoreDatalakeTests : KeyStoreTests
{
    public KeyStoreDatalakeTests(ITestOutputHelper output) : base(output) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("KeyStoreTests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
