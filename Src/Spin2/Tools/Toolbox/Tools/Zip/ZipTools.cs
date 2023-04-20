using System.IO.Compression;
using System.Text;

namespace Toolbox.Tools.Zip;

public static class ZipTools
{
    public static void Write(this ZipArchive zipArchive, string path, string data)
    {
        data.NotEmpty();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        zipArchive.Write(path, memoryStream);
    }

    public static void Write(this ZipArchive zipArchive, string path, byte[] data)
    {
        data.NotNull();

        using var memoryStream = new MemoryStream(data);
        zipArchive.Write(path, memoryStream);
    }

    public static void Write(this ZipArchive zipArchive, string path, Stream sourceStream)
    {
        zipArchive.NotNull();
        path.NotEmpty();
        sourceStream.NotNull();

        ZipArchiveEntry entry = zipArchive!.CreateEntry(path);

        using StreamWriter writer = new StreamWriter(entry.Open());
        sourceStream.CopyTo(writer.BaseStream);
    }

    public static bool Exist(this ZipArchive zipArchive, string path)
    {
        zipArchive.NotNull();
        path.NotEmpty();

        ZipArchiveEntry? zipArchiveEntry = zipArchive!.Entries
            .Where(x => x.FullName == path)
            .FirstOrDefault();

        return zipArchiveEntry != null;
    }

    public static string ReadAsString(this ZipArchive zipArchive, string path)
    {
        zipArchive.NotNull();
        using var memoryStream = new MemoryStream();

        zipArchive.Read(path, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
    }

    public static byte[] Read(this ZipArchive zipArchive, string path)
    {
        zipArchive.NotNull();

        using var memoryStream = new MemoryStream();
        zipArchive.Read(path, memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream.ToArray();
    }

    public static void Read(this ZipArchive zipArchive, string path, Stream targetStream)
    {
        zipArchive.NotNull();
        path.NotEmpty();
        targetStream.NotNull();

        ZipArchiveEntry? entry = zipArchive!.GetEntry(path);
        entry.NotNull(name: $"{path} does not exist in zip");

        using StreamReader writer = new StreamReader(entry.Open());
        writer.BaseStream.CopyTo(targetStream);
    }
}
