namespace TicketShareWeb.Application;

public class NotifyChange
{
    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
}
