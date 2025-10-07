//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;


//public enum GrantColumn
//{
//    None,
//    Name,
//    Role,
//    Principal,
//}

//internal record GiGrantSelect : IGraphInstruction
//{
//    public GrantColumn GrantColumn { get; init; }
//    public string Value { get; init; } = null!;
//}

//internal static class GiGrantSelectTool
//{
//    /// example: select grant where {columnName} = "{value}"
//    /// column name = name, role, principal
//    public static Option<IGraphInstruction> Build(InterContext ic)
//    {
//        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

//        var s1 = ic.ProcessSymbols(["select-sym", "grant-sym", "where-sym"]);
//        if (s1.IsError()) return s1.ToOptionStatus<IGraphInstruction>();

//        var columnName = ic.GetEnum<GrantColumn>("name-sym", "role-sym", "principal-sym");
//        if (columnName.IsError()) return columnName.ToOptionStatus<IGraphInstruction>();

//        ic.ProcessSymbols(["equal"]);

//        var valueToken = ic.GetValue("column-value");
//        if (valueToken.IsError()) return valueToken.ToOptionStatus<IGraphInstruction>();

//        scope.Cancel();
//        return new GiGrantSelect()
//        {
//            GrantColumn = columnName.Return(),
//            Value = valueToken.Return(),
//        };
//    }

//    public static string GetCommandDesc(this GiGrantSelect subject)
//    {
//        var command = new[]
//        {
//            nameof(GiGrantSelect),
//            $"GrantColumn={subject.GrantColumn}",
//            $"Value={subject.Value}"
//        }.Join(", ");

//        return command;
//    }
//}