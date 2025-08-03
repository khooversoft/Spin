using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data.DataClient;

public class LockManagerExclusiveTests : Test.Data.DataClient.LockManagerExclusiveTests
{
    public LockManagerExclusiveTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("LockManagerExclusiveTests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
