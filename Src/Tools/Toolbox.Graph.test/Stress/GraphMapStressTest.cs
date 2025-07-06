using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Stress;

public class GraphMapStressTest
{
    private readonly ITestOutputHelper _output;

    public GraphMapStressTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task StressTest()
    {
        const int taskCount = 5;
        var map = new GraphMap();
        var token = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        Task<bool>[] list = Enumerable.Range(0, taskCount)
            .SelectMany(x => new IWorker[] { new WorkerTwoNodesAndEdge(map, _output, x), new WorkerThreeNodesAndEdges(map, _output, x) }, (o, i) => i)
            .Select(x => x.Run(token, NullScopeContext.Instance))
            .ToArray();

        _output.WriteLine("Starting tasks");
        bool[] result = await Task.WhenAll(list);
        _output.WriteLine("Finished tasks");
    }
}
