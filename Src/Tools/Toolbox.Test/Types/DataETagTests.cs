﻿using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class DataETagTests
{
    [Fact]
    public void EmptyDataETagEqual()
    {
        var d1 = Array.Empty<byte>();
        DataETag e1 = new DataETag(d1);
        DataETag e2 = new DataETag(d1);

        (e1 == e2).Should().BeTrue();

        string data = e1.ToJson();
        data.Should().NotBeEmpty();
        DataETag e3 = data.ToObject<DataETag>().NotNull();

        (e1 == e3).Should().BeTrue();
        e3.Data.Should().BeEquivalentTo(e1.Data);
        e3.ETag.Should().BeEquivalentTo(e1.ETag);
    }

    [Fact]
    public void EmptyDataETagWithEmptyTagEqual()
    {
        var d1 = Array.Empty<byte>();
        DataETag e1 = new DataETag(d1, "");
        DataETag e2 = new DataETag(d1, "");

        (e1 == e2).Should().BeTrue();

        string data = e1.ToJson();
        data.Should().NotBeEmpty();
        DataETag e3 = data.ToObject<DataETag>().NotNull();

        (e1 == e3).Should().BeTrue();
        e3.Data.Should().BeEquivalentTo(e1.Data);
        e3.ETag.Should().BeEquivalentTo(e1.ETag);
    }

    [Fact]
    public void DataETagEqual()
    {
        var d1 = "hello".ToBytes();
        DataETag e1 = new DataETag(d1);
        DataETag e2 = new DataETag(d1);

        (e1 == e2).Should().BeTrue();

        string data = e1.ToJson();
        data.Should().NotBeEmpty();
        DataETag e3 = data.ToObject<DataETag>().NotNull();

        (e1 == e3).Should().BeTrue();
        e3.Data.Should().BeEquivalentTo(e1.Data);
        e3.ETag.Should().BeEquivalentTo(e1.ETag);
    }

    [Fact]
    public void DataETagWithEmptyTagEqual()
    {
        var d1 = "hello".ToBytes();
        DataETag e1 = new DataETag(d1, "FF");
        DataETag e2 = new DataETag(d1, "FF");

        (e1 == e2).Should().BeTrue();

        string data = e1.ToJson();
        data.Should().NotBeEmpty();
        DataETag e3 = data.ToObject<DataETag>().NotNull();

        (e1 == e3).Should().BeTrue();
        e3.Data.Should().BeEquivalentTo(e1.Data);
        e3.ETag.Should().BeEquivalentTo(e1.ETag);
    }
}
