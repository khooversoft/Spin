using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OptionTests
{
    [Fact]
    public void TestNone()
    {
        (Option<int>.None == Option<int>.None).BeTrue();
        (5.ToOption() != Option<int>.None).BeTrue();
        (Option<int>.None != 5.ToOption()).BeTrue();

        (5.ToOption() == 5.ToOption()).BeTrue();

        (new Option<string>("value") == new Option<string>("value")).BeTrue();
        (new Option<string>("value") == "value".ToOption()).BeTrue();
        (new Option<int?>(null) == new Option<int?>()).BeTrue();
        (new Option<int?>(null) != new Option<int?>(5)).BeTrue();

        new Option<int>(10).Equals("hello").BeFalse();
        new Option<string>("hello").Equals("hello").BeTrue();

        (new Option<string>() == new Option<string>()).BeTrue();
        (new Option<string>() == new Option<string>("hello")).BeFalse();

        int? v1 = null;
        Option<int?> o1 = v1.ToOption();
        o1.Return(false).BeNull();
        o1.HasValue.BeFalse();
        o1.StatusCode.Be(StatusCode.NoContent);
        (o1 == Option<int?>.None).BeTrue();
        (o1 == default).BeTrue();
        (o1 != default).BeFalse();

        int v2 = 0;
        Option<int> o2 = v2.ToOption();
        o2.Return().Be(0);
        o2.HasValue.BeTrue();
        o2.Return().Be(0);
        o2.StatusCode.Be(StatusCode.OK);
        (o2 == Option<int>.None).BeFalse();
        (o2 == default).BeFalse();
        (o2 != default).BeTrue();

        int v3 = 5;
        Option<int> o3 = v3.ToOption();
        o3.HasValue.BeTrue();
        o3.Return().Be(5);
        o3.StatusCode.Be(StatusCode.OK);
        (o3 == Option<int>.None).BeFalse();
        (o3 == default).BeFalse();
        (o3 != default).BeTrue();
    }

    [Fact]
    public void ValueTypes()
    {
        long v1 = 10;
        Option<long> optionV1 = v1.ToOption();
        optionV1.StatusCode.Be(StatusCode.OK);
        (optionV1 != default).BeTrue();
        optionV1.HasValue.BeTrue();
        optionV1.Value.Be(v1);

        DateTime v2 = DateTime.Now;
        Option<DateTime> optionV2 = v2.ToOption();
        optionV2.StatusCode.Be(StatusCode.OK);
        (optionV2 != default).BeTrue();
        optionV2.HasValue.BeTrue();
        optionV2.Value.Assert(x => x == v2);
    }

    [Fact]
    public void NullableValueWithValueTypes()
    {
        long? v1 = 10;
        Option<long?> optionV1 = v1.ToOption();
        (optionV1 != default).BeTrue();
        optionV1.StatusCode.Be(StatusCode.OK);
        optionV1.HasValue.BeTrue();
        optionV1.Value.Be(v1);

        DateTime? v2 = DateTime.Now;
        Option<DateTime?> optionV2 = v2.ToOption();
        (optionV2 != default).BeTrue();
        optionV2.StatusCode.Be(StatusCode.OK);
        optionV2.HasValue.BeTrue();
        optionV2.Value.Assert(x => x == v2);
    }

    [Fact]
    public void NullableValueWithNullTypes()
    {
        long? v1 = null;
        Option<long?> optionV1 = v1.ToOption();
        (optionV1 == default).BeTrue();
        optionV1.StatusCode.Be(StatusCode.NoContent);
        optionV1.HasValue.BeFalse();
        optionV1.Value.Be(null);

        DateTime? v2 = null;
        Option<DateTime?> optionV2 = v2.ToOption();
        (optionV2 == default).BeTrue();
        optionV2.StatusCode.Be(StatusCode.NoContent);
        optionV2.HasValue.BeFalse();
        optionV2.Value.Assert(x => x == v2);
    }

    [Fact]
    public void CollectionResponseValueType()
    {
        var v1 = Array.Empty<int>();
        Option<int> v2 = v1.FirstOrDefault().ToOption();
        v2.StatusCode.Be(StatusCode.OK);
        (v2 == default).BeFalse();
        (v2 != default).BeTrue();
        (v2 == Option<int>.None).BeFalse();
        (v2 != Option<int>.None).BeTrue();

        var v3 = new[] { 1, 2, 3 };
        var v4 = v3.FirstOrDefault().ToOption();
        v4.StatusCode.Be(StatusCode.OK);
        v4.Return().Be(1);
        (v4 != default).BeTrue();
        (v4 == default).BeFalse();
        (v4 != Option<int>.None).BeTrue();
        (v4 == Option<int>.None).BeFalse();

        var v5 = v3.LastOrDefault().ToOption();
        v5.Return().Be(3);
    }

    [Fact]
    public void CollectionResponseReferenceType()
    {
        var v1 = Array.Empty<string>();
        Option<string> v2 = v1.FirstOrDefault().ToOption();
        v2.Return(false).BeNull();
        v2.StatusCode.Be(StatusCode.NoContent);
        (v2 == default).BeTrue();
        (v2 != default).BeFalse();
        (v2 == Option<string>.None).BeTrue();
        (v2 != Option<string>.None).BeFalse();

        var v3 = new[] { "1", "2", "3" };
        var v4 = v3.FirstOrDefault().ToOption();
        v4.StatusCode.Be(StatusCode.OK);
        v4.Return().Be("1");
        (v4 != default).BeTrue();
        (v4 == default).BeFalse();
        (v4 != Option<string>.None).BeTrue();
        (v4 == Option<string>.None).BeFalse();

        var v5 = v3.LastOrDefault().ToOption();
        v5.Return().Be("3");
    }

    [Fact]
    public void CollectionResponseCollectionType()
    {
        var v1 = Array.Empty<string>();
        Option<string[]> v2 = v1.ToOption();
        (v2 == default).BeFalse();
        v2.Return().Length.Be(0);

        Option<IReadOnlyList<string>> v3 = ((IReadOnlyList<string>)v1.ToArray()).ToOption();
        v2.Return().Length.Be(0);

        var v4 = new[]
        {
            "first",
            "second",
            "third"
        };

        var s1 = v4.ToOption();
        s1.Return().Length.Be(3);
        s1.Return().Skip(1).FirstOrDefault().Be("second");

        v4.Skip(1).FirstOrDefault().ToOption().Return().Be("second");

        Option<string> s3 = v4.Where(x => x == "abc").FirstOrDefault().ToOption();
        (s3 == default).BeTrue();

        Verify.Throws<ArgumentException>(() => s3.Return().Be(default));
        (s3 == Option<string>.None).BeTrue();
    }

    [Fact]
    public void TypeConversionTest()
    {
        var str = "hello".ToOption();
        var length = str.Bind(x => x.Length);
        length.Return().Be(5);
    }

    [Fact]
    public void OptionSerialization()
    {
        new Option(StatusCode.OK).Action(x =>
        {
            var j1 = x.ToJson();
            Option o2 = j1.ToObject<Option>();
            x.StatusCode.Be(StatusCode.OK);
            x.Error.BeNull();
        });

        new Option(StatusCode.NotFound, "Record not found").Action(x =>
        {
            var j1 = x.ToJson();
            Option o2 = j1.ToObject<Option>();
            x.StatusCode.Be(StatusCode.NotFound);
            x.Error.Be("Record not found");
        });
    }

    [Fact]
    public void Option_T_Serialization()
    {
        var r = new Record1
        {
            Name = "name1",
            Value = 10,
        };

        new Option<Record1>(r, StatusCode.OK).Action(x =>
        {
            var j1 = x.ToJson();
            Option<Record1> o2 = j1.ToObject<Option<Record1>>();
            x.StatusCode.Be(StatusCode.OK);
            x.Error.BeNull();
            (x.Value == r).BeTrue();
        });

        new Option<Record1>(StatusCode.NotFound, "Record not found").Action(x =>
        {
            var j1 = x.ToJson();
            Option<Record1> o2 = j1.ToObject<Option<Record1>>();
            x.StatusCode.Be(StatusCode.NotFound);
            x.Error.Be("Record not found");
            x.Value.BeNull();
        });
    }

    private record Record1
    {
        public string Name { get; init; } = null!;
        public int Value { get; init; }
    }
}
