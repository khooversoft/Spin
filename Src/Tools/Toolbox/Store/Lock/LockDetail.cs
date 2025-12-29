//using Toolbox.Tools;

//namespace Toolbox.Store;

//public record LockDetail
//{
//    public LockDetail(string key, IFileLeasedAccess fileLeasedAccess, LockMode lockMode, TimeSpan duration)
//    {
//        Key = key.NotEmpty();
//        Path = fileLeasedAccess.Path.NotEmpty();
//        FileLeasedAccess = fileLeasedAccess.NotNull();
//        LockMode = lockMode;
//        Duration = duration.Assert(x => x.TotalSeconds > 1, x => $"Invalid duration={x}");
//    }

//    public string Key { get; }
//    public string Path { get; }
//    public IFileLeasedAccess FileLeasedAccess { get; }
//    public LockMode LockMode { get; }
//    public DateTime AcquiredDate { get; } = DateTime.UtcNow;
//    public TimeSpan Duration { get; }

//    public static string CreateKey(string pipelineName, string path) => $"{pipelineName.NotEmpty()}:{path.NotEmpty()}";
//}
