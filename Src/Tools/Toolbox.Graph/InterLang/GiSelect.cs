﻿using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal sealed record GiSelect : IGraphInstruction
{
    public IReadOnlyList<ISelectInstruction> Instructions { get; init; } = Array.Empty<ISelectInstruction>();
    public IReadOnlyList<string> DataNames { get; init; } = Array.Empty<string>();

    public bool Equals(GiSelect? obj)
    {
        bool result = obj is GiSelect subject &&
            Enumerable.SequenceEqual(Instructions, subject.Instructions) &&
            Enumerable.SequenceEqual(DataNames, subject.DataNames);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Instructions, DataNames);
}

internal static class GiSelectTool
{
    private static readonly Func<InterContext, Option<ISelectInstruction>>[] _call = [
        GiNodeSelectTool.Build,
        GiEdgeSelectTool.Build,
        GiFullJoinTool.Build,
        GiLeftJoinTool.Build,
        GiReturnNamesTool.Build,
    ];

    public static Option<IGraphInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        // select (
        if (!interContext.Cursor.TryGetValue(out var selectValue) || selectValue.Token.Value != "select") return (StatusCode.NotFound, "no 'select' command found");

        var instructions = new Sequence<ISelectInstruction>();

        while (interContext.Cursor.TryPeekValue(out var nextToken))
        {
            if (nextToken.MetaSyntaxName == "term")
            {
                interContext.Cursor.MoveNext();
                continue;
            };

            var result = _call
                .Select(x => x(interContext))
                .SkipWhile(x => x.IsError())
                .Select(x => x.Return())
                .FirstOrDefaultOption(true);

            if (result.IsError()) return (StatusCode.BadRequest, "Failed to parse");
            instructions += result.Return();
        }

        scope.Cancel();
        return new GiSelect
        {
            Instructions = instructions.ToImmutableArray(),
        };
    }
}