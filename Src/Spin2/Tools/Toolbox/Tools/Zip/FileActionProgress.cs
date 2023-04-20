namespace Toolbox.Tools.Zip;

public struct FileActionProgress
{
    public FileActionProgress(int total, int count)
    {
        Total = total;
        Count = count;
    }

    public int Total { get; }
    public int Count { get; }

    public override string ToString() => $"{{ Total={Total}, Count={Count} }}";
}
