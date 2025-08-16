using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Store;

public class LockManagerExclusiveTests : Test.Store.LockManagerExclusiveTests
{
    public LockManagerExclusiveTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("LockManagerExclusiveTests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
