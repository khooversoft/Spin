using System.IO.Compression;
using Toolbox.Extensions;

namespace Toolbox.Tools.Zip;

public static class ZipExtractExtension
{
    public static void ExtractToFolder(this ZipArchive zipArchive, string toFolder, CancellationToken token, Action<FileActionProgress>? monitor)
    {
        zipArchive.NotNull();
        toFolder.NotEmpty();

        if (Directory.Exists(toFolder)) Directory.Delete(toFolder);
        Directory.CreateDirectory(toFolder);

        FileEntry[] zipFiles = zipArchive.Entries
            .Where(x => !x.FullName.EndsWith("/"))
            .Select(x => new FileEntry(x))
            .ToArray();

        int fileCount = 0;
        foreach (FileEntry zipFile in zipFiles)
        {
            if (token.IsCancellationRequested) break;

            monitor?.Invoke(new FileActionProgress(zipFiles.Length, ++fileCount));

            Path.Combine(toFolder, zipFile.FilePath
                .Assert(x => !x.StartsWith("\\"), $"Invalid zip file path {zipFile.FilePath}"))
                .Action(x => zipFile.ExtractToFile(x));
        }
    }

    public static void CompressFiles(this ZipArchive zipArchive, CopyTo[] files, CancellationToken token = default, Action<FileActionProgress>? monitor = null)
    {
        files.Assert(x => x.Length > 0, "No fileFolder(s) specified");

        int fileCount = 0;
        foreach (CopyTo file in files)
        {
            if (token.IsCancellationRequested) return;

            zipArchive.CreateEntryFromFile(file.Source, file.Destination);

            monitor?.Invoke(new FileActionProgress(files.Length, ++fileCount));
        }
    }


    private struct FileEntry
    {
        public FileEntry(ZipArchiveEntry zipArchiveEntry)
        {
            FilePath = zipArchiveEntry.FullName.Replace("/", @"\");
            ZipArchiveEntry = zipArchiveEntry;
        }

        public string FilePath { get; }

        public ZipArchiveEntry ZipArchiveEntry { get; }

        public void ExtractToFile(string filePath)
        {
            filePath.NotEmpty();

            ZipArchiveEntry.ExtractToFile(filePath, true);
        }
    }
}
