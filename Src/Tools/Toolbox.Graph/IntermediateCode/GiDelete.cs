using System.Collections.Immutable;
using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed record GiDelete : IGraphInstruction
{
    public IReadOnlyList<ISelectInstruction> Instructions { get; init; } = Array.Empty<ISelectInstruction>();

    public bool Equals(GiDelete? obj)
    {
        bool result = obj is GiDelete subject &&
            Enumerable.SequenceEqual(Instructions, subject.Instructions);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Instructions);
}

internal static class GiDeleteTool
{
    private static readonly Func<InterContext, Option<ISelectInstruction>>[] _call = [
        GiNodeSelectTool.Build,
        GiEdgeSelectTool.Build,
        GiFullJoinTool.Build,
        GiLeftJoinTool.Build,
    ];

    public static Option<IGraphInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var selectValue) || selectValue.Token.Value != "delete") return (StatusCode.NotFound, "no 'delete' command found");

        var instructions = new Sequence<ISelectInstruction>();

        while (interContext.Cursor.TryPeekValue(out var nextToken))
        {
            if (nextToken.MetaSyntaxName == "term") break;

            var result = _call
                .Select(x => x(interContext))
                .SkipWhile(x => x.IsError())
                .Select(x => x.Return())
                .FirstOrDefaultOption(true);

            if (result.IsError()) return (StatusCode.BadRequest, "Failed to parse");
            instructions += result.Return();
        }

        scope.Cancel();
        return new GiDelete
        {
            Instructions = instructions.ToImmutableArray(),
        };
    }


    public static string GetCommandDesc(this GiDelete subject)
    {
        var command = nameof(GiDelete).ToEnumerable()
            .Concat(subject.Instructions.Select(x => "{ " + getInstructionCommand(x) + " }"))
            .Join(", ");

        return command;

        string getInstructionCommand(ISelectInstruction instruction)
        {
            string command = instruction switch
            {
                GiNodeSelect node => node.GetCommandDesc(),
                GiEdgeSelect edge => edge.GetCommandDesc(),
                GiFullJoin full => full.GetCommandDesc(),
                GiLeftJoin left => left.GetCommandDesc(),
                GiRightJoin right => right.GetCommandDesc(),

                _ => throw new UnreachableException(),
            };

            return command;
        }
    }
}

