using System.IO.Compression;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Tools;

public class ZipMemoryTests
{
    [Fact]
    public void GivenData_WhenSavedFile_RoundTriped_ShouldSucceed()
    {
        const string data = "This is a test";
        const string file = "file1.txt";

        using var writeBuffer = new MemoryStream();

        using (var zipWrite = new ZipArchive(writeBuffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            zipWrite.Write(file, data);
        }

        writeBuffer.Seek(0, SeekOrigin.Begin);

        using (var read = new ZipArchive(writeBuffer, ZipArchiveMode.Read, leaveOpen: true))
        {
            string readData = read.ReadAsString(file);
            readData.Should().Be(data);
        }
    }
}
