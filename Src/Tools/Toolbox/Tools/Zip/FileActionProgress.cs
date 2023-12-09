namespace Toolbox.Tools;

public struct FileActionProgress
{
    public FileActionProgress(int total, int count, CopyTo copyTo)
    {
        Total = total;
        Count = count;
        CopyTo = copyTo;
    }

    public int Total { get; }
    public int Count { get; }
    public CopyTo CopyTo { get; }

    public override string ToString() => $"Total={Total}, Count={Count}, CopyTo={CopyTo}";
}
