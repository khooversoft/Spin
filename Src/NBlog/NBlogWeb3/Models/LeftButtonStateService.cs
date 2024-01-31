namespace NBlogWeb3.Models;

public class LeftButtonStateService
{
    private const int Hide = 0;
    private const int Show = 1;
    private int _showMenu = Hide;

    public bool Get() => (_showMenu == Show);

    public bool Toggle()
    {
        Interlocked.CompareExchange(ref _showMenu, _showMenu ^ 1, _showMenu);
        NotifyStateChange();
        return Get();
    }

    public void Clear()
    {
        Interlocked.Exchange(ref _showMenu, Hide);
        NotifyStateChange();
    }

    public event Action? OnChange;

    private void NotifyStateChange() => OnChange?.Invoke();
}
