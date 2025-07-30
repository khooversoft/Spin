using Microsoft.Extensions.DependencyInjection;
using Toolbox.Azure.test.Application;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data;

public class LockManagerExclusiveTests : Toolbox.Test.Data.DataClient.LockManagerExclusiveTests
{
    public LockManagerExclusiveTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    protected override void AddStore(IServiceCollection services)
    {
        var datalakeOption = TestApplication.ReadOption("LockManagerExclusiveTests");
        services.AddDatalakeFileStore(datalakeOption);
    }
}
