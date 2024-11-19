using Xunit.Abstractions;

namespace Toolbox.Graph.test.Stress;

public class GraphCommandStressTests
{
    private readonly ITestOutputHelper _output;

    public GraphCommandStressTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task StressTest()
    {
        const int taskCount = 10;
        GraphTestClient graphHost = GraphTestStartup.CreateGraphTestHost();
        var context = graphHost.GetScopeContext<GraphCommandStressTests>();

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        Task<bool>[] list = Enumerable.Range(0, taskCount)
            .SelectMany(x => new IWorker[] { new CommandTwoNodesAndEdges(graphHost, _output, x), new CommandThreeNodesAndEdges(graphHost, _output, x) }, (o, i) => i)
            .Select(x => x.Run(token, context))
            .ToArray();

        _output.WriteLine("Starting tasks");
        bool[] result = await Task.WhenAll(list);
        _output.WriteLine("Finished tasks");
    }
}
