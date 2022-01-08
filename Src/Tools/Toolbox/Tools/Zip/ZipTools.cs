using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Models;

namespace Toolbox.Tools.Zip;

public static class ZipTools
{
    public static void Write(this ZipArchive zipArchive, string path, string data)
    {
        data.VerifyNotEmpty(nameof(data));

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        zipArchive.Write(path, memoryStream);
    }

    public static void Write(this ZipArchive zipArchive, string path, byte[] data)
    {
        data.VerifyNotNull(nameof(data));

        using var memoryStream = new MemoryStream(data);
        zipArchive.Write(path, memoryStream);
    }

    public static void Write(this ZipArchive zipArchive, string path, Stream sourceStream)
    {
        zipArchive.VerifyNotNull(nameof(zipArchive));
        path.VerifyNotEmpty(nameof(path));
        sourceStream.VerifyNotNull(nameof(sourceStream));

        ZipArchiveEntry entry = zipArchive!.CreateEntry(path);

        using StreamWriter writer = new StreamWriter(entry.Open());
        sourceStream.CopyTo(writer.BaseStream);
    }

    public static bool Exist(this ZipArchive zipArchive, string path)
    {
        zipArchive.VerifyNotNull(nameof(zipArchive));
        path.VerifyNotEmpty(nameof(path));

        ZipArchiveEntry? zipArchiveEntry = zipArchive!.Entries
            .Where(x => x.FullName == path)
            .FirstOrDefault();

        return zipArchiveEntry != null;
    }

    public static string ReadAsString(this ZipArchive zipArchive, string path)
    {
        zipArchive.VerifyNotNull(nameof(zipArchive));
        using var memoryStream = new MemoryStream();

        zipArchive.Read(path, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
    }

    public static byte[] Read(this ZipArchive zipArchive, string path)
    {
        zipArchive.VerifyNotNull(nameof(zipArchive));

        using var memoryStream = new MemoryStream();
        zipArchive.Read(path, memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream.ToArray();
    }

    public static void Read(this ZipArchive zipArchive, string path, Stream targetStream)
    {
        zipArchive.VerifyNotNull(nameof(zipArchive));
        path.VerifyNotEmpty(nameof(path));
        targetStream.VerifyNotNull(nameof(targetStream));

        ZipArchiveEntry? entry = zipArchive!.GetEntry(path);
        entry.VerifyNotNull($"{path} does not exist in zip");

        using StreamReader writer = new StreamReader(entry.Open());
        writer.BaseStream.CopyTo(targetStream);
    }
}
