using Azure;
using Toolbox.Extensions;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class DataETagTests
{
    [Fact]
    public void EmptyDataETagEqual()
    {
        var d1 = Array.Empty<byte>();
        DataETag e1 = new DataETag(d1);
        DataETag e2 = new DataETag(d1);

        (e1 == e2).Should().BeTrue();
    }

    [Fact]
    public void OneEmptyDataETagEqual()
    {
        var d1 = Array.Empty<byte>();
        DataETag e1 = new DataETag(d1);

        byte[] d2 = "simple".ToBytes();
        DataETag e2 = new DataETag(d2);

        (e1 == e2).Should().BeFalse();
    }

    [Fact]
    public void SimpleDataETagEqualSameData()
    {
        byte[] d1 = "simple".ToBytes();
        DataETag e1 = new DataETag(d1);
        DataETag e2 = new DataETag(d1);

        (e1 == e2).Should().BeTrue();
    }

    [Fact]
    public void SimpleDataETagEqual()
    {
        byte[] d1 = "simple".ToBytes();
        DataETag e1 = new DataETag(d1);
        byte[] d2 = "simple".ToBytes();
        DataETag e2 = new DataETag(d2);

        (e1 == e2).Should().BeTrue();
    }

    [Fact]
    public void SimpleDataETagNotEqual()
    {
        byte[] d1 = "simple".ToBytes();
        DataETag e1 = new DataETag(d1);
        byte[] d2 = "simple2".ToBytes();
        DataETag e2 = new DataETag(d2);

        (e1 == e2).Should().BeFalse();
    }

    [Fact]
    public void SimpleDataETag()
    {
        byte[] d1 = "simple".ToBytes();
        ETag t1 = new ETag("hello");
        DataETag e1 = new DataETag(d1, t1.ToString());

        byte[] d2 = "simple".ToBytes();
        ETag t2 = new ETag("hello");
        DataETag e2 = new DataETag(d2, t2.ToString());

        (e1 == e2).Should().BeTrue();
    }
}
