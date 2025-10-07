//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;


//public enum GroupColumn
//{
//    None,
//    Name,
//    Member,
//}

//internal record GiGroupSelect : IGraphInstruction
//{
//    public GroupColumn GroupColumn { get; init; }
//    public string Value { get; init; } = null!;
//}

//internal static class GiGroupSelectTool
//{
//    /// example: select group where {columnName} = "{value}"
//    /// column name = name, member
//    public static Option<IGraphInstruction> Build(InterContext ic)
//    {
//        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

//        var s1 = ic.ProcessSymbols(["select-sym", "group-sym", "where-sym"]);
//        if (s1.IsError()) return s1.ToOptionStatus<IGraphInstruction>();

//        var columnName = ic.GetEnum<GroupColumn>("name-sym", "member-sym");
//        if (columnName.IsError()) return columnName.ToOptionStatus<IGraphInstruction>();

//        var s2 = ic.IsSymbol("equal");
//        if( s2.IsError()) return s2.ToOptionStatus<IGraphInstruction>();

//        var valueToken = ic.GetValue("column-value");
//        if (valueToken.IsError()) return valueToken.ToOptionStatus<IGraphInstruction>();

//        scope.Cancel();
//        return new GiGroupSelect()
//        {
//            GroupColumn = columnName.Return(),
//            Value = valueToken.Return(),
//        };
//    }

//    public static string GetCommandDesc(this GiGroupSelect subject)
//    {
//        var command = new[]
//        {
//            nameof(GiGroupSelect),
//            $"GroupColumn={subject.GroupColumn}",
//            $"Value={subject.Value}"
//        }.Join(", ");

//        return command;
//    }
//}