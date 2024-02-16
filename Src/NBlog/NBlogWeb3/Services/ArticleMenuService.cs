namespace NBlogWeb3.Services;

public class ArticleMenuService
{
    private string? _articleId;

    public string? Get() => _articleId;

    public string? Set(string? articleId)
    {
        _articleId = articleId;
        NotifyStateChange();
        return Get();
    }

    public void Clear()
    {
        _articleId = null;
        NotifyStateChange();
    }

    public event Action? OnChange;

    private void NotifyStateChange() => OnChange?.Invoke();
}
