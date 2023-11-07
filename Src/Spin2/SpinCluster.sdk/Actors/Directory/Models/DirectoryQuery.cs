using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryQuery
{
    public DirectoryQuery() { }
    public DirectoryQuery(string graphQuery) => GraphQuery = graphQuery;

    [Id(0)] public string GraphQuery { get; init; } = null!;

    public static IValidator<DirectoryQuery> Validator { get; } = new Validator<DirectoryQuery>()
        .RuleFor(x => x.GraphQuery).NotEmpty()
        .Build();
}


public static class DirectoryQueryExtensions
{
    public static Option Validate(this DirectoryQuery subject) => DirectoryQuery.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DirectoryQuery subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
