namespace TicketShareWeb.Components.Pages.Profile;

public class DialogResultState<T>
{
    public DialogResultState(T value)
    {
        Value = value;
    }

    public DialogResultState(T value, bool isDelete)
    {
        Value = value;
        IsDelete = isDelete;
    }

    public T? Value { get; init; }
    public bool IsDelete { get; init; }
}