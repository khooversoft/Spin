using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinCluster.sdk.Actors.Contract;

public class ContractQueryBuilder
{
    public string? PrincipalId { get; set; }
    public IList<QueryBlockType> BlockTypes { get; } = new List<QueryBlockType>();

    public ContractQueryBuilder SetPrincipalId(string principalId) => this.Action(x => x.PrincipalId = principalId);
    public ContractQueryBuilder Add(params QueryBlockType[] types) => this.Action(x => types.ForEach(y => x.BlockTypes.Add(y)));

    public ContractQueryBuilder Add<T>(bool latestOnly = false)
    {
        Add(new QueryBlockType
        {
            BlockType = typeof(T).GetTypeName(),
            LatestOnly = latestOnly,
        });

        return this;
    }

    public ContractQuery Build()
    {
        PrincipalId.NotEmpty("required");
        BlockTypes.NotNull();

        return new ContractQuery
        {
            PrincipalId = PrincipalId,
            BlockTypes = BlockTypes.ToArray(),
        };
    }
}
