using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Toolbox.Model;
using Toolbox.Models;

namespace Toolbox.Tools.Zip
{
    public class ZipFile
    {
        private readonly string _zipFilePath;

        public ZipFile(string zipFilePath)
        {
            zipFilePath.VerifyNotEmpty(nameof(zipFilePath));

            _zipFilePath = zipFilePath;
        }

        public string ExpandToTempFile(CancellationToken token)
        {
            string toFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(_zipFilePath));
            ExpandFiles(toFolder, token);

            return toFolder;
        }

        public void ExpandFiles(string toFolder, CancellationToken token, Action<FileActionProgress>? monitor = null)
        {
            _zipFilePath.VerifyAssert(x => File.Exists(x), $"{_zipFilePath} does not exist");

            using var stream = new FileStream(_zipFilePath, FileMode.Open);
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false);

            zipArchive.ExtractToFolder(toFolder, token, monitor);
        }

        public void CompressFiles(CancellationToken token, Action<FileActionProgress>? monitor = null, params CopyTo[] files)
        {
            files.VerifyAssert(x => x.Length > 0, "No fileFolder(s) specified");

            using var stream = new FileStream(_zipFilePath, FileMode.Create);
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, false);

            int fileCount = 0;
            foreach (CopyTo file in files)
            {
                if (token.IsCancellationRequested) return;

                zipArchive.CreateEntryFromFile(file.Source, file.Destination);

                monitor?.Invoke(new FileActionProgress(files.Length, ++fileCount));
            }
        }
    }
}