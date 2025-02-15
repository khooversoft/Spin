using System.Buffers.Text;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class InterLangAnalysis
{
    public static Option ValidateInstructions(IReadOnlyList<IGraphInstruction> instructions)
    {
        var selectResult = instructions
            .OfType<GiSelect>()
            .Select(x => ValidateSelect(x.Instructions))
            .SkipWhile(x => x.IsOk())
            .Take(1)
            .ToArray();

        if (selectResult.Length > 0) return selectResult[0];

        var deleteResult = instructions
            .OfType<GiDelete>()
            .Select(x => ValidateSelect(x.Instructions))
            .SkipWhile(x => x.IsOk())
            .Take(1)
            .ToArray();

        if (deleteResult.Length > 0) return deleteResult[0];

        var base64Result = ValidateBase64(instructions);
        if (base64Result.IsError()) return base64Result;

        return StatusCode.OK;
    }

    private static Option ValidateSelect(IReadOnlyList<ISelectInstruction> instructions)
    {
        const int netural = 0;
        const int node = 1;
        const int edge = 2;

        int direction = netural;
        var stack = instructions.Reverse().ToStack();

        while (stack.TryPop(out var value))
        {
            switch (value)
            {
                case GiNodeSelect:
                    if (direction == node) return (StatusCode.BadRequest, "Cannot have 2 select nodes joined, must be joined with left or full join");
                    direction = node;
                    break;

                case GiEdgeSelect:
                    if (direction == edge) return (StatusCode.BadRequest, "Cannot have 2 select edges joined, must be joined with left or full join");
                    direction = edge;
                    break;

                case GiFullJoin:
                case GiLeftJoin:
                case GiRightJoin:
                case GiReturnNames:
                    break;

                default:
                    return (StatusCode.BadRequest, $"Unknown instruction {value}");
            }
            ;
        }

        return StatusCode.OK;
    }

    private static Option ValidateBase64(IReadOnlyList<IGraphInstruction> instructions)
    {
        var result = instructions
            .OfType<GiNode>()
            .SelectMany(x => x.Data)
            .Select(x => (x.Key, isValid: Base64.IsValid(x.Value)))
            .Where(x => x.isValid == false)
            .Select(x => x.Key)
            .Join(", ");

        if (result.IsNotEmpty()) return (StatusCode.BadRequest, $"Invalid base64 for keys={result}");
        return StatusCode.OK;
    }
}
