using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public enum FileType
{
    Unknown = 0,
    Manifest,
    Document,
    Configuration,
    ContactMe,
    SysData,
    About,
}

public record BuildContext
{
    public required string BasePath { get; init; }
    public required string PackageFile { get; init; }
    public required string WorkingFolder { get; init; }
    public Sequence<(FileType FileType, string FilePath)> Files { get; set; } = Array.Empty<(FileType FileType, string FilePath)>();
    public Sequence<ManifestFile> ManifestFiles { get; set; } = Array.Empty<ManifestFile>();
    public Sequence<(string sourceFile, string zipFile)> CopyCommands { get; set; } = Array.Empty<(string sourceFile, string zipFile)>();
    public Sequence<string> Errors { get; set; } = Array.Empty<string>();

    public string CreateWorkFile(string file) => Path.Combine(WorkingFolder, file.NotEmpty());
    public string RemoveBasePath(string file) => Path.Combine(WorkingFolder, file[(BasePath.Length + 1)..]);
}
