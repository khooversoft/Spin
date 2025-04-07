using Xunit.Abstractions;

namespace Toolbox.Graph.test.Stress;

public class GraphCommandStressTests
{
    private readonly ITestOutputHelper _outputHelper;

    public GraphCommandStressTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task StressTest()
    {
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<GraphCommandStressTests>();
        const int taskCount = 10;

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        Task<bool>[] list = Enumerable.Range(0, taskCount)
            .SelectMany(x => new IWorker[] { new CommandTwoNodesAndEdges(graphTestClient, _outputHelper, x), new CommandThreeNodesAndEdges(graphTestClient, _outputHelper, x) }, (o, i) => i)
            .Select(x => x.Run(token, context))
            .ToArray();

        _outputHelper.WriteLine("Starting tasks");
        bool[] result = await Task.WhenAll(list);
        _outputHelper.WriteLine("Finished tasks");
    }
}
