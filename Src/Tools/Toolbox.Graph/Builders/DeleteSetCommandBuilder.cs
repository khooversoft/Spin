using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.Builders;

public class DeleteSetCommandBuilder
{
    public IList<ISelectSearch> Searches { get; } = new List<ISelectSearch>();
    public DeleteSetCommandBuilder AddSearch(ISelectSearch search) => this.Action(_ => Searches.Add(search.NotNull()));
    public DeleteSetCommandBuilder AddLeftJoin() => this.Action(_ => Searches.Add(new LeftJoinSearch()));
    public DeleteSetCommandBuilder AddFullJoin() => this.Action(_ => Searches.Add(new FullJoinSearch()));

    public DeleteSetCommandBuilder AddNodeSearch(Action<NodeSearch> search)
    {
        var nodeSearch = new NodeSearch();
        search(nodeSearch);
        Searches.Add(nodeSearch);
        return this;
    }

    public DeleteSetCommandBuilder AddEdgeSearch(Action<EdgeSearch> search)
    {
        var edgeSearch = new EdgeSearch();
        search(edgeSearch);
        Searches.Add(edgeSearch);
        return this;
    }

    public string Build()
    {
        var seq = new string?[][]
        {
            ["delete"],
            [.. Searches.Select(x => x.Build())],
            [";"]
        };

        string cmd = seq.SelectMany(x => x).Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}
