using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphCode<T>
{
    private readonly IGraphSchema<T> _graphSchema;
    private readonly T _subject;
    private T? _currentSubject;

    public GraphCode(T subject, IGraphSchema<T> graphSchema)
    {
        _subject = subject.NotNull();
        _graphSchema = graphSchema.NotNull();
    }

    public GraphCode<T> SetCurrent(T currentSubject) => this.Action(x => x._currentSubject = currentSubject);

    public IReadOnlyList<string> BuildSetCommands() => new Sequence<string>()
        + SetNodeCommand()
        + SetIndexCommands()
        + SetReferenceCommands();

    public IReadOnlyList<string> BuildDeleteCommands() => new Sequence<string>()
        + DeleteNodeCommand()
        + DeleteIndexCommands()
        + DeleteReferenceCommands();

    public string BuildSelectCommand(string queryName = "default")
    {
        var result = _graphSchema.SchemaValues.GetSelectCommand(_subject, queryName);
        return result;
    }

    public IReadOnlyList<string> SetNodeCommand()
    {
        string nodeKey = _graphSchema.SchemaValues.GetNodeKey(_subject);
        string? dataName = _graphSchema.SchemaValues.GetNodeDataName(_subject);
        string? tags = _graphSchema.SchemaValues.GetTags(_subject);

        string base64 = _subject.ToJson64();
        var cmd = GraphTool.SetNodeCommand(nodeKey, tags, base64, dataName);
        return [cmd];
    }

    public IReadOnlyList<string> DeleteNodeCommand()
    {
        string nodeKey = _graphSchema.SchemaValues.NotNull().GetNodeKey(_subject);
        return [GraphTool.DeleteNodeCommand(nodeKey)];
    }

    public IReadOnlyList<string> SetIndexCommands()
    {
        IReadOnlyList<string> currentIndexKeys = _graphSchema.SchemaValues.GetValues(_currentSubject, SchemaType.Index);
        IReadOnlyList<string> indexKeys = _graphSchema.SchemaValues.GetValues(_subject, SchemaType.Index);

        string nodeKey = _graphSchema.SchemaValues.GetNodeKey(_subject);

        var cmds = new Sequence<string>();
        cmds += currentIndexKeys.Except(indexKeys, StringComparer.OrdinalIgnoreCase).Select(x => GraphTool.DeleteNodeCommand(x));
        cmds += indexKeys.Except(currentIndexKeys, StringComparer.OrdinalIgnoreCase).SelectMany(x => GraphTool.CreateIndexCommands(nodeKey, x));

        return cmds;
    }

    public IReadOnlyList<string> DeleteIndexCommands()
    {
        IReadOnlyList<string> indexKeys = _graphSchema.SchemaValues.GetValues(_subject, SchemaType.Index);

        var cmds = new Sequence<string>();
        cmds += indexKeys.Select(x => GraphTool.DeleteNodeCommand(x));

        return cmds;
    }

    public IReadOnlyList<string> SetReferenceCommands()
    {
        IReadOnlyList<string> currentIndexKeys = _graphSchema.SchemaValues.GetValues(_currentSubject, SchemaType.Reference);
        IReadOnlyList<string> indexKeys = _graphSchema.SchemaValues.GetValues(_subject, SchemaType.Reference);

        string nodeKey = _graphSchema.SchemaValues.GetNodeKey(_subject);

        var cmds = new Sequence<string>();

        cmds += currentIndexKeys
            .Except(indexKeys, StringComparer.OrdinalIgnoreCase)
            .SelectMany(x => PraseAttribute(x).Func(y => GraphTool.DeleteEdgeCommands(nodeKey, y.value, y.attribute)));

        cmds += indexKeys
            .Except(currentIndexKeys, StringComparer.OrdinalIgnoreCase)
            .SelectMany(x => PraseAttribute(x).Func(y => GraphTool.CreateEdgeCommands(nodeKey, y.value, y.attribute, null)));

        return cmds;
    }

    public IReadOnlyList<string> DeleteReferenceCommands()
    {
        IReadOnlyList<string> indexKeys = _graphSchema.SchemaValues.GetValues(_subject, SchemaType.Reference);

        string nodeKey = _graphSchema.SchemaValues.GetNodeKey(_subject);

        var cmds = new Sequence<string>();
        cmds += indexKeys.SelectMany(x => PraseAttribute(x).Func(y => GraphTool.DeleteEdgeCommands(nodeKey, y.value, y.attribute)));

        return cmds;
    }

    private static (string attribute, string value) PraseAttribute(string value) => value.Split('`') switch
    {
        { Length: 2 } v => (v[0], v[1]),
        _ => throw new ArgumentException($"Invalid edge value: {value}"),
    };
}

public static class GraphCodeCommands
{
    public static GraphCode<T> Code<T>(this IGraphSchema<T> graphSchema, T subject) => new GraphCode<T>(subject, graphSchema);

    public static IReadOnlyList<string> GetValues<T>(this IReadOnlyList<ISchemaValue<T>> graphValue, T? subject, SchemaType schemaType) => subject switch
    {
        null => ImmutableArray<string>.Empty,

        _ => schemaType switch
        {
            SchemaType.Reference => graphValue.NotNull()
                .Where(x => x.Type == schemaType)
                .Select(x => (value: x.GetResolvedValue(subject), attribute: x.Attribute))
                .Where(x => x.value.IsNotEmpty() && x.attribute.IsNotEmpty())
                .Select(x => $"{x.attribute}`{x.value}")
                .ToImmutableArray(),

            _ => graphValue.NotNull()
                .Where(x => x.Type == schemaType)
                .Select(x => x.GetResolvedValue(subject))
                .OfType<string>()
                .ToImmutableArray(),
        }
    };
}
