using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal record GiSelectObject : IGraphInstruction
{
    public string ObjectName { get; init; } = null!;
    public string AttributeName { get; init; } = null!;
    public string Value { get; init; } = null!;
}


internal static class GiSelectObjectTool
{
    public static Option<IGraphInstruction> Build(InterContext ic)
    {
        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

        var s1 = ic.IsSymbol("select-sym");
        if (s1.IsError()) return s1.ToOptionStatus<IGraphInstruction>();

        var objectName = ic.GetValue("object-name");
        if (objectName.IsError()) return objectName.ToOptionStatus<IGraphInstruction>();

        var s2 = ic.IsSymbol("where-sym");
        if (s2.IsError()) return s2.ToOptionStatus<IGraphInstruction>();

        var attributeName = ic.GetValue("attribute-name");
        if (attributeName.IsError()) return attributeName.ToOptionStatus<IGraphInstruction>();

        var s3 = ic.IsSymbol("equal");
        if (s3.IsError()) return s3.ToOptionStatus<IGraphInstruction>();

        var objectValue = ic.GetValue("object-value");
        if (objectValue.IsError()) return objectValue.ToOptionStatus<IGraphInstruction>();

        scope.Cancel();
        return new GiSelectObject()
        {
            ObjectName = objectName.Return(),
            AttributeName = attributeName.Return(),
            Value = objectValue.Return(),
        };
    }

    public static string GetCommandDesc(this GiSelectObject subject)
    {
        var command = new[]
        {
            nameof(GiSelectObject),
            $"ObjectName={subject.ObjectName}",
            $"AttributeName={subject.AttributeName}",
            $"Value={subject.Value}"
        }.Join(", ");

        return command;
    }
}
