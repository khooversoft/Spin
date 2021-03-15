using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Toolbox.Tools;

namespace Toolbox.Tools.Zip
{
    public class ZipWriter : IDisposable
    {
        private ZipArchive? _zipArchive;

        public ZipWriter(ZipArchive zipArchive)
        {
            zipArchive.VerifyNotNull(nameof(zipArchive));

            _zipArchive = zipArchive;
        }

        public ZipWriter(string filePath)
        {
            filePath.VerifyNotEmpty(nameof(filePath));

            FilePath = filePath;
        }

        public string? FilePath { get; }

        public ZipWriter OpenFile()
        {
            _zipArchive.VerifyAssert(x => x == null, "Zip archive already opened");

            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            if (File.Exists(FilePath!))
            {
                File.Delete(FilePath!);
            }

            _zipArchive = System.IO.Compression.ZipFile.Open(FilePath!, ZipArchiveMode.Create);
            return this;
        }

        public void Close()
        {
            var archive = Interlocked.Exchange(ref _zipArchive, null!);
            archive?.Dispose();
        }

        public ZipWriter Write(string zipPath, string data)
        {
            data.VerifyNotEmpty(nameof(data));

            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            return Write(zipPath, memoryStream);
        }

        public ZipWriter Write(string zipPath, Stream sourceStream)
        {
            zipPath.VerifyNotEmpty(nameof(zipPath));
            sourceStream.VerifyNotNull(nameof(sourceStream));
            _zipArchive.VerifyNotNull("Not opened");

            ZipArchiveEntry entry = _zipArchive!.CreateEntry(zipPath);

            using StreamWriter writer = new StreamWriter(entry.Open());
            sourceStream.CopyTo(writer.BaseStream);

            return this;
        }

        public void Dispose() => Close();
    }
}