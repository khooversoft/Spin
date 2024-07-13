using System.Collections.Immutable;
using Toolbox.Tools;

namespace Toolbox.Graph;

public record GraphCodeContext<T>
{
    public GraphCodeContext(IEnumerable<ISchemaValue<T>> graphValues) => GraphValues = graphValues.NotNull().ToImmutableArray();

    public IReadOnlyList<ISchemaValue<T>> GraphValues { get; }
    public T? Subject { get; init; }
    public T? OldSubject { get; init; }
}
