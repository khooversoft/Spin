namespace Toolbox.Graph;

public interface IChangeTrace
{
    void Log(ChangeTrx trx);
    Task LogAsync(ChangeTrx trx);
}

//public static class ChangeTraceExtensions
//{
//    public static void LogNode(this IChangeTrace trace, ChangeTrxType trxType, Guid logKey, GraphNode currentValue, GraphNode? updateValue)
//    {
//        ChangeTrx trx = new ChangeTrx(trxType, logKey, currentValue, updateValue);
//        trace.Log(trx);
//    }

//    public static Task LogNodeAsync(this IChangeTrace trace, ChangeTrxType trxType, Guid logKey, GraphNode currentValue, GraphNode? updateValue)
//    {
//        ChangeTrx trx = new ChangeTrx(trxType, logKey, currentValue, updateValue);
//        return trace.LogAsync(trx);
//    }

//    public static void LogEdge(this IChangeTrace trace, ChangeTrxType trxType, Guid logKey, GraphEdge currentValue, GraphEdge? updateValue)
//    {
//        ChangeTrx trx = new ChangeTrx(trxType, logKey, currentValue, updateValue);
//        trace.LogAsync(trx);
//    }

//    public static Task LogEdgeAsync(this IChangeTrace trace, ChangeTrxType trxType, Guid logKey, GraphEdge currentValue, GraphEdge? updateValue)
//    {
//        ChangeTrx trx = new ChangeTrx(trxType, logKey, currentValue, updateValue);
//        return trace.LogAsync(trx);
//    }
//}
