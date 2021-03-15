using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Toolbox.Tools;

namespace Toolbox.Tools.Zip
{
    public class ZipReader : IDisposable
    {
        private ZipArchive? _zipArchive;

        public ZipReader(ZipArchive zipArchive)
        {
            zipArchive.VerifyNotNull(nameof(zipArchive));

            _zipArchive = zipArchive;
        }

        public ZipReader(string filePath)
        {
            filePath.VerifyNotEmpty(nameof(filePath));

            FilePath = filePath;
        }

        public string? FilePath { get; }

        public ZipReader OpenFile()
        {
            _zipArchive.VerifyAssert(x => x == null, "Zip archive already opened");
            File.Exists(FilePath).VerifyAssert(x => x == true, $"{FilePath} does not exist");

            _zipArchive = System.IO.Compression.ZipFile.Open(FilePath!, ZipArchiveMode.Read);

            return this;
        }

        public void Close()
        {
            var archive = Interlocked.Exchange(ref _zipArchive, null!);
            archive?.Dispose();
        }

        public bool Exist(string zipPath)
        {
            _zipArchive.VerifyNotNull("Zip archive is not opened");
            zipPath.VerifyNotEmpty(nameof(zipPath));

            ZipArchiveEntry? zipArchiveEntry = _zipArchive!.Entries
                .Where(x => x.FullName == zipPath)
                .FirstOrDefault();

            return zipArchiveEntry != null;
        }

        public string Read(string zipPath)
        {
            using var memoryStream = new MemoryStream();

            Read(zipPath, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        public ZipReader Read(string zipPath, Stream targetStream)
        {
            zipPath.VerifyNotEmpty(nameof(zipPath));
            targetStream.VerifyNotNull(nameof(targetStream));
            _zipArchive.VerifyNotNull("Not opened");

            ZipArchiveEntry? entry = _zipArchive!.GetEntry(zipPath);
            entry.VerifyNotNull($"{zipPath} does not exist in zip");

            using StreamReader writer = new StreamReader(entry.Open());
            writer.BaseStream.CopyTo(targetStream);

            return this;
        }

        public void Dispose() => Close();
    }
}