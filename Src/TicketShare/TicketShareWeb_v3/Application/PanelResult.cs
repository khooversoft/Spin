namespace TicketShareWeb.Application;

public record PanelResult<T>
{
    private PanelResult(bool doDelete, T? value)
    {
        DoDelete = doDelete;
        Value = value;
    }

    public bool DoDelete { get; }
    public T? Value { get; }

    public static PanelResult<T> Set(T value) => new PanelResult<T>(false, value);
    public static PanelResult<T> Delete() => new PanelResult<T>(true, default);
}
