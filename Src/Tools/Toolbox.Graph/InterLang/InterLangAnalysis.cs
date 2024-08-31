using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class InterLangAnalysis
{
    public static Option ValidateInstructions(IReadOnlyList<IGraphInstruction> instructions)
    {
        var selectResult = instructions.OfType<GiSelect>().All(x => ValidateSelect(x.Instructions).IsOk());
        var deleteResult = instructions.OfType<GiDelete>().All(x => ValidateSelect(x.Instructions).IsOk());

        return (selectResult && deleteResult) ? StatusCode.OK : StatusCode.BadRequest;
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
                    if (direction == node) return StatusCode.BadRequest;
                    direction = node;
                    break;

                case GiEdgeSelect:
                    if (direction == edge) return StatusCode.BadRequest;
                    direction = edge;
                    break;

                case GiFullJoin:
                case GiLeftJoin:
                case GiReturnNames:
                    break;

                default:
                    return (StatusCode.BadRequest, $"Unknown instruction {value}");
            };
        }

        return StatusCode.OK;
    }
}
