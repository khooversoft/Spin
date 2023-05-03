using FluentAssertions;
using Toolbox.Types.Maybe;

namespace Toolbox.Test.Types;

public class OptionTTests
{
    [Fact]
    public void TestNone()
    {
        (Option<int>.None == Option<int>.None).Should().BeTrue();
        ((false, default(int)).ToOption() == Option<int>.None).Should().BeTrue();
        (5.ToOption() != Option<int>.None).Should().BeTrue();
        (Option<int>.None != 5.ToOption()).Should().BeTrue();

        (5.ToOption() == 5.ToOption()).Should().BeTrue();

        (new Option<string>("value") == new Option<string>("value")).Should().BeTrue();
        (new Option<string>("value") == "value".ToOption()).Should().BeTrue();
        (new Option<int?>(null) == new Option<int?>()).Should().BeTrue();
        (new Option<int?>(null) != new Option<int?>(5)).Should().BeTrue();

        new Option<int>(10).Equals("hello").Should().BeFalse();
        new Option<string>("hello").Equals("hello").Should().BeTrue();

        (new Option<string>() == new Option<string>()).Should().BeTrue();
        (new Option<string>() == new Option<string>("hello")).Should().BeFalse();

        int? v1 = null;
        Option<int?> o1 = v1.ToOption();
        o1.Return().Should().BeNull();
        o1.HasValue.Should().BeFalse();
        o1.StatusCode.Should().Be(OptionStatus.NoContent);
        (o1 == Option<int?>.None).Should().BeTrue();
        (o1 == default).Should().BeTrue();
        (o1 != default).Should().BeFalse();

        int v2 = 0;
        Option<int> o2 = v2.ToOption();
        o2.Return().Should().Be(0);
        o2.HasValue.Should().BeTrue();
        o2.Return().Should().Be(0);
        o2.StatusCode.Should().Be(OptionStatus.OK);
        (o2 == Option<int>.None).Should().BeFalse();
        (o2 == default).Should().BeFalse();
        (o2 != default).Should().BeTrue();

        int v3 = 5;
        Option<int> o3 = v3.ToOption();
        o3.HasValue.Should().BeTrue();
        o3.Return().Should().Be(5);
        o3.StatusCode.Should().Be(OptionStatus.OK);
        (o3 == Option<int>.None).Should().BeFalse();
        (o3 == default).Should().BeFalse();
        (o3 != default).Should().BeTrue();
    }

    [Fact]
    public void ValueTypes()
    {
        long v1 = 10;
        Option<long> optionV1 = v1.ToOption();
        optionV1.StatusCode.Should().Be(OptionStatus.OK);
        (optionV1 != default).Should().BeTrue();
        optionV1.HasValue.Should().BeTrue();
        optionV1.Value.Should().Be(v1);

        DateTime v2 = DateTime.Now;
        Option<DateTime> optionV2 = v2.ToOption();
        optionV2.StatusCode.Should().Be(OptionStatus.OK);
        (optionV2 != default).Should().BeTrue();
        optionV2.HasValue.Should().BeTrue();
        optionV2.Value.Should().Be(v2);
    }

    [Fact]
    public void NullableValueWithValueTypes()
    {
        long? v1 = 10;
        Option<long?> optionV1 = v1.ToOption();
        (optionV1 != default).Should().BeTrue();
        optionV1.StatusCode.Should().Be(OptionStatus.OK);
        optionV1.HasValue.Should().BeTrue();
        optionV1.Value.Should().Be(v1);

        DateTime? v2 = DateTime.Now;
        Option<DateTime?> optionV2 = v2.ToOption();
        (optionV2 != default).Should().BeTrue();
        optionV2.StatusCode.Should().Be(OptionStatus.OK);
        optionV2.HasValue.Should().BeTrue();
        optionV2.Value.Should().Be(v2);
    }

    [Fact]
    public void NullableValueWithNullTypes()
    {
        long? v1 = null;
        Option<long?> optionV1 = v1.ToOption();
        (optionV1 == default).Should().BeTrue();
        optionV1.StatusCode.Should().Be(OptionStatus.NoContent);
        optionV1.HasValue.Should().BeFalse();
        optionV1.Value.Should().Be(null);

        DateTime? v2 = null;
        Option<DateTime?> optionV2 = v2.ToOption();
        (optionV2 == default).Should().BeTrue();
        optionV2.StatusCode.Should().Be(OptionStatus.NoContent);
        optionV2.HasValue.Should().BeFalse();
        optionV2.Value.Should().Be(v2);
    }

    [Fact]
    public void CollectionResponseValueType()
    {
        var v1 = Array.Empty<int>();
        Option<int> v2 = v1.FirstOrDefault().ToOption();
        v2.StatusCode.Should().Be(OptionStatus.OK);
        (v2 == default).Should().BeFalse();
        (v2 != default).Should().BeTrue();
        (v2 == Option<int>.None).Should().BeFalse();
        (v2 != Option<int>.None).Should().BeTrue();

        var v3 = new[] { 1, 2, 3 };
        var v4 = v3.FirstOrDefault().ToOption();
        v4.StatusCode.Should().Be(OptionStatus.OK);
        v4.Return().Should().Be(1);
        (v4 != default).Should().BeTrue();
        (v4 == default).Should().BeFalse();
        (v4 != Option<int>.None).Should().BeTrue();
        (v4 == Option<int>.None).Should().BeFalse();

        var v5 = v3.LastOrDefault().ToOption();
        v5.Return().Should().Be(3);
    }

    [Fact]
    public void CollectionResponseReferenceType()
    {
        var v1 = Array.Empty<string>();
        Option<string> v2 = v1.FirstOrDefault().ToOption();
        v2.Return().Should().BeNull();
        v2.StatusCode.Should().Be(OptionStatus.NoContent);
        (v2 == default).Should().BeTrue();
        (v2 != default).Should().BeFalse();
        (v2 == Option<string>.None).Should().BeTrue();
        (v2 != Option<string>.None).Should().BeFalse();

        var v3 = new[] { "1", "2", "3" };
        var v4 = v3.FirstOrDefault().ToOption();
        v4.StatusCode.Should().Be(OptionStatus.OK);
        v4.Return().Should().Be("1");
        (v4 != default).Should().BeTrue();
        (v4 == default).Should().BeFalse();
        (v4 != Option<string>.None).Should().BeTrue();
        (v4 == Option<string>.None).Should().BeFalse();

        var v5 = v3.LastOrDefault().ToOption();
        v5.Return().Should().Be("3");
    }

    [Fact]
    public void CollectionResponseCollectionType()
    {
        var v1 = Array.Empty<string>();
        Option<string[]> v2 = v1.ToOption();
        (v2 == default).Should().BeFalse();
        v2.Return().Length.Should().Be(0);

        Option<IReadOnlyList<string>> v3 = ((IReadOnlyList<string>)v1.ToArray()).ToOption();
        v2.Return().Length.Should().Be(0);

        var v4 = new[]
        {
            "first",
            "second",
            "third"
        };

        var s1 = v4.ToOption();
        s1.Return().Length.Should().Be(3);
        s1.Return().Skip(1).FirstOrDefault().Should().Be("second");

        v4.Skip(1).FirstOrDefault().ToOption().Return().Should().Be("second");

        Option<string> s3 = v4.Where(x => x == "abc").FirstOrDefault().ToOption();
        (s3 == default).Should().BeTrue();
        s3.Return().Should().Be(default);
        (s3 == Option<string>.None).Should().BeTrue();
    }

    [Fact]
    public void TypeConversionTest()
    {
        var str = "hello".ToOption();
        var length = str.Bind(x => x.Length);
        length.Return().Should().Be(5);
    }
}
