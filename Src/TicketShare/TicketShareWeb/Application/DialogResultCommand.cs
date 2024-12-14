//using Toolbox.Types;

//namespace TicketShareWeb.Application;

//public readonly struct DialogResultCommand<T>
//{
//     DialogResultCommand(T value)
//    {
//        StatusCode = StatusCode.OK;
//        Value = value;
//    }

//    public DialogResultCommand(StatusCode statusCode)
//    {
//        StatusCode = statusCode;
//    }

//    public DialogResultCommand(T value, bool isDelete)
//    {
//        StatusCode = StatusCode.OK;
//        Value = value;
//        IsDelete = isDelete;
//    }

//    public StatusCode StatusCode { get; init; }

//    public T? Value { get; init; }
//    public bool IsDelete { get; init; }

//    public static DialogResultCommand<T> Ok(T value) => new DialogResultCommand<T>(value);
//    public static DialogResultCommand<T> Error() => new DialogResultCommand<T>(StatusCode.Conflict);
//    public static DialogResultCommand<T> Delete(T value) => new DialogResultCommand<T>(value, true);
//}
