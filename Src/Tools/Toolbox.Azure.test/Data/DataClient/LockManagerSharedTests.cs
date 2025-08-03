using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data.DataClient;

public class LockManagerSharedTests : Test.Data.DataClient.LockManagerSharedTests
{
    public LockManagerSharedTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("LockManagerSharedTests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
