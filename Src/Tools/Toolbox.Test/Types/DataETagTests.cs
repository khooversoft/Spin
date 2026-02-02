using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class DataETagTests
{
    private record TestPayload(string Name, int Count);

    [Fact]
    public void TestSerializerRegistery()
    {
        JsonSerializerContextRegistered.Find<DataETag>().BeOk();
    }

    [Fact]
    public void EmptyDataETagEqual()
    {
        var d1 = Array.Empty<byte>();
        DataETag e1 = new DataETag(d1);
        DataETag e2 = new DataETag(d1);

        (e1 == e2).BeTrue();

        string data = e1.ToJson();
        data.NotEmpty();
        DataETag e3 = data.ToObject<DataETag>().NotNull();

        (e1 == e3).BeTrue();
        Enumerable.SequenceEqual(e3.Data, e1.Data).BeTrue();
        e3.ETag.Be(e1.ETag);
    }

    [Fact]
    public void EqualitySameDataWithoutETag()
    {
        byte[] d1 = "hello".ToBytes();
        DataETag e1 = new DataETag(d1);
        DataETag e2 = new DataETag("hello".ToBytes());

        (e1 == e2).BeTrue();
    }

    [Fact]
    public void EqualityWithSameDataPreservesETag()
    {
        var d1 = "hello".ToBytes();
        DataETag e1 = new DataETag(d1, "FF");

        string data = e1.ToJson();
        DataETag e2 = data.ToObject<DataETag>().NotNull();

        (e1 == e2).BeTrue();
        e2.ETag.Be("FF");
    }

    [Fact]
    public void EqualityDiffersWhenOneIsEmpty()
    {
        DataETag empty = new DataETag(Array.Empty<byte>());
        DataETag populated = new DataETag("simple".ToBytes());

        (empty == populated).BeFalse();
    }

    [Fact]
    public void EqualityDiffersOnDifferentData()
    {
        DataETag e1 = new DataETag("simple".ToBytes());
        DataETag e2 = new DataETag("simple2".ToBytes());

        (e1 == e2).BeFalse();
    }

    [Fact]
    public void EqualityIgnoresETag()
    {
        var data = "same".ToBytes();
        DataETag e1 = new DataETag(data, "etag-1");
        DataETag e2 = new DataETag(data, "etag-2");

        (e1 == e2).BeTrue();
    }

    [Fact]
    public void ImplicitConversionFromBytes()
    {
        DataETag tag = "hello".ToBytes();

        tag.DataToString().Be("hello");
        (tag.ETag == null).BeTrue();
    }

    [Fact]
    public void ValidateEmptyDataFails()
    {
        var tag = new DataETag(Array.Empty<byte>());
        var result = tag.Validate();

        result.IsBadRequest().BeTrue();
        (result.Error?.Contains("Data 0 is invalid") ?? false).BeTrue();
    }

    [Fact]
    public void ValidatePopulatedDataSucceeds()
    {
        var tag = new DataETag("ok".ToBytes());

        tag.Validate().IsOk().BeTrue();
    }

    [Fact]
    public void AppendConcatenatesDataAndDropsETag()
    {
        DataETag left = new DataETag("left".ToBytes(), "etag-left");
        DataETag right = new DataETag("right".ToBytes(), "etag-right");

        DataETag combined = left + right;

        combined.DataToString().Be("leftright");
        (combined.ETag == null).BeTrue();
    }

    [Fact]
    public void StripETagRemovesOnlyETag()
    {
        DataETag tag = new DataETag("payload".ToBytes(), "etag-value");

        DataETag stripped = tag.StripETag();

        stripped.DataToString().Be("payload");
        (stripped.ETag == null).BeTrue();
    }

    [Fact]
    public void WithETagOverridesValue()
    {
        DataETag tag = new DataETag("payload".ToBytes(), "old");

        DataETag updated = tag.WithETag("new");

        updated.DataToString().Be("payload");
        updated.ETag.Be("new");
    }

    [Fact]
    public void WithHashSetsHashAsETag()
    {
        var data = "hash-me".ToBytes();
        var expectedHash = data.ToHexHash();

        DataETag tagged = new DataETag(data).WithHash();

        tagged.ETag.Be(expectedHash);
        tagged.DataToString().Be("hash-me");
    }

    [Fact]
    public void ToDataETagWithHashAddsHash()
    {
        var expectedHash = "abc".ToBytes().ToHexHash();

        DataETag tagged = "abc".ToDataETagWithHash();

        tagged.DataToString().Be("abc");
        tagged.ETag.Be(expectedHash);
    }

    [Fact]
    public void ToObjectRoundTripsPayload()
    {
        var payload = new TestPayload("alpha", 42);

        DataETag data = payload.ToDataETag();
        TestPayload roundtrip = data.ToObject<TestPayload>();

        (roundtrip == payload).BeTrue();
    }

    [Fact]
    public void JsonSerializationRoundTripPreservesETag()
    {
        var original = new DataETag("payload".ToBytes(), "etag-value");

        string json = original.ToJson();
        DataETag roundtrip = json.ToObject<DataETag>();
        roundtrip.NotNull();

        roundtrip.DataToString().Be("payload");
        roundtrip.ETag.Be("etag-value");
        (roundtrip == original).BeTrue();
    }
}
