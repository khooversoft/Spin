using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph.Extensions.Testing;
using Xunit.Abstractions;

namespace Toolbox.Graph.Extensions.test.Application;

public static class TestHost
{
    public static async Task<GraphHostService> Create(ITestOutputHelper outputHelper)
    {
        var result = await GraphTestStartup.CreateGraphService(
            config: x =>
            {
                x.AddGraphExtensions();
                x.AddSingleton<TestAuthStateProvider>();
            },
            logOutput: x => outputHelper.WriteLine(x)
        );

        return result;
    }
}
