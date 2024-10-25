using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;

public class SelectCommandBuilder
{
    public HashSet<string> DataNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public IList<ISelectSearch> Searches { get; } = new List<ISelectSearch>();

    public SelectCommandBuilder AddSearch(ISelectSearch search) => this.Action(_ => Searches.Add(search.NotNull()));
    public SelectCommandBuilder AddLeftJoin() => this.Action(_ => Searches.Add(new LeftJoinSearch()));
    public SelectCommandBuilder AddFullJoin() => this.Action(_ => Searches.Add(new FullJoinSearch()));

    public SelectCommandBuilder AddNodeSearch(Action<NodeSearch> search)
    {
        var nodeSearch = new NodeSearch();
        search(nodeSearch);
        Searches.Add(nodeSearch);
        return this;
    }

    public SelectCommandBuilder AddEdgeSearch(Action<EdgeSearch> search)
    {
        var edgeSearch = new EdgeSearch();
        search(edgeSearch);
        Searches.Add(edgeSearch);
        return this;
    }

    public SelectCommandBuilder AddDataName(string name)
    {
        name.NotEmpty();

        DataNames.Add(name);
        return this;
    }

    public string Build()
    {
        string dataNames = DataNames.OrderBy(x => x).Join(",");

        string? returnOpr = dataNames.IsNotEmpty() ? "return " + dataNames : null;

        var seq = new string?[][]
        {
            ["select"],
            [.. Searches.Select(x => x.Build())],
            [returnOpr],
            [";"]
        };

        string cmd = seq.SelectMany(x => x).Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}
