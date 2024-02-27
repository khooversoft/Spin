using System.Text.Json.Serialization;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class TagsTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("-", false)]
    [InlineData(".", false)]
    [InlineData(":", false)]
    [InlineData("#", false)]
    [InlineData("*", true)]
    [InlineData("k", true)]
    [InlineData("k1", true)]
    [InlineData("k1/", false)]
    [InlineData("k.1", true)]
    [InlineData("k-1", true)]
    [InlineData("k:1", true)]
    [InlineData("1", true)]
    [InlineData("1#", false)]
    [InlineData("1k", true)]
    [InlineData("1k.", true)]
    [InlineData("1k.v", true)]
    public void IsKeyValid(string? key, bool expected)
    {
        bool actual = TagsTool.IsKeyValid(key, out Option _);
        actual.Should().Be(expected);

        if (expected || key.IsEmpty())
        {
            _ = new Tags(key);
            _ = new Tags().Set(key);
            return;
        }

        Action act = () => new Tags(key);
        act.Should().Throw<ArgumentException>();

        Action act2 = () => new Tags().Set(key);
        act2.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("a=v;b=v", false)]
    [InlineData("a=v/b=v", false)]
    [InlineData("a=v-b=v", false)]
    [InlineData("*", true)]
    [InlineData("a", true)]
    [InlineData("a=v", true)]
    [InlineData("a,b", true)]
    [InlineData("a=v,b", true)]
    [InlineData("a,b=v", true)]
    [InlineData("a=v,b=v", true)]
    [InlineData("a,b,c", true)]
    [InlineData("a=v,b,c", true)]
    [InlineData("a=v,b=v,c", true)]
    [InlineData("a=v,b=v,c=v", true)]
    [InlineData("a,b=v,c=v", true)]
    [InlineData("a,b,c=v", true)]
    [InlineData("  a=v   ,b  =  v  ,c =   v   ", true)]
    public void IsSetValid(string? key, bool expected)
    {
        if (expected || key.IsEmpty())
        {
            _ = new Tags(key);
            _ = new Tags().Set(key);
            return;
        }

        Action act = () => new Tags(key);
        act.Should().Throw<ArgumentException>();

        Action act2 = () => new Tags().Set(key);
        act2.Should().Throw<ArgumentException>();
    }


    [Fact]
    public void ImplicitConversion()
    {
        Tags t = "hello";
        t.Should().NotBeNull();
        t.ToString().Should().Be("hello");
    }

    [Fact]
    public void TagsEmpty()
    {
        var tags = new Tags();
        tags.Count.Should().Be(0);

        var tags2 = new Tags();
        tags2.Count.Should().Be(0);

        (tags == tags2).Should().BeTrue();
        (tags.ToString() == tags2.ToString()).Should().BeTrue();

        var tags3 = new Tags("");
        (tags == tags3).Should().BeTrue();

        var tags4 = new Tags((string)null!);
        (tags == tags4).Should().BeTrue();
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("k", true)]
    [InlineData("key=node1", true)]
    [InlineData("key=node1, t1, t2=v2", true)]
    public void EqualPatterns(string? line, bool shouldExpect)
    {
        var t1 = new Tags(line);
        var t2 = new Tags(line);

        (t1 == t2).Should().Be(shouldExpect);
        (t1 != t2).Should().Be(!shouldExpect);
    }

    [Fact]
    public void TagsSingleAndNotEqual()
    {
        var tags = new Tags().Set("key1").Action(x =>
        {
            x.Count.Should().Be(1);
            x.ContainsKey("key1").Should().BeTrue();
            x["key1"].Should().BeNull();
            x.Has("key1").Should().BeTrue();
            x.Has("key1", "value").Should().BeFalse();
            x.Has("fake").Should().BeFalse();
            x.Has("key1", "fake1").Should().BeFalse();
            x.Has(null).Should().BeFalse();
        });

        var tags2 = new Tags().Set("key1=value1").Action(x =>
        {
            x.Count.Should().Be(1);
            x.ContainsKey("key1").Should().BeTrue();
            x["key1"].Should().Be("value1");
            x.Has("key1").Should().BeTrue();
            x.Has("key1", "value1").Should().BeTrue();
            x.Has("fake").Should().BeFalse();
            x.Has("key1", "fake1").Should().BeFalse();
        });

        (tags != tags2).Should().BeTrue();
    }

    [Fact]
    public void TagsKeyValue()
    {
        var tags = new Tags();
        tags["key2"] = "value2";
        tags["key1"] = "value1";
        tags.Count.Should().Be(2);
        tags.ContainsKey("key1").Should().BeTrue();

        var tags2 = new Tags();
        tags2["key1"] = "value1";
        tags2["key2"] = "value2";
        tags2.Count.Should().Be(2);

        (tags == tags2).Should().BeTrue();
        (tags.ToString() == tags2.ToString()).Should().BeTrue();

        var tags3 = new Tags("key2=value2,key1=value1");
        (tags == tags3).Should().BeTrue();

        var tags4 = new Tags("key2=value2");
        (tags == tags4).Should().BeFalse();
    }


    [Fact]
    public void RemoveTag()
    {
        var tags = new Tags();
        tags["key2"] = "value2";
        tags["key1"] = "value1";
        tags.Count.Should().Be(2);
        tags.ContainsKey("key1").Should().BeTrue();

        tags.Set("-key2");
        tags.ToString().Should().Be("key1=value1");

        tags.Set("key2=value3");
        tags.ToString().Should().Be("key1=value1,key2=value3");

        tags.Set("-key1");
        tags.ToString().Should().Be("key2=value3");
    }

    [Fact]
    public void TagsUsingSet()
    {
        var tags = new Tags();
        tags.Set("key2=value2");
        tags.Set("key1=value1");
        tags.Count.Should().Be(2);

        var tags2 = new Tags();
        tags2["key1"] = "value1";
        tags2["key2"] = "value2";
        tags2.Count.Should().Be(2);
        (tags == tags2).Should().BeTrue();
        (tags.ToString() == tags2.ToString()).Should().BeTrue();

        var tags3 = new Tags();
        tags3.Set("key2=value2,key1=value1");
        tags3.Count.Should().Be(2);
        (tags == tags3).Should().BeTrue();
        (tags.ToString() == tags3.ToString()).Should().BeTrue();

        var tags4 = new Tags("key2=value2,key1=value1");
        tags4.Count.Should().Be(2);
        (tags == tags4).Should().BeTrue();
        (tags.ToString() == tags4.ToString()).Should().BeTrue();
    }

    [Fact]
    public void TagsUsingTags()
    {
        var tags = new Tags();
        tags.Set("key2");
        tags.Set("key1=value1");
        tags.Count.Should().Be(2);
        tags.ContainsKey("key2").Should().BeTrue();

        var tags2 = new Tags();
        tags2["key1"] = "value1";
        tags2["key2"] = null;
        tags2.Count.Should().Be(2);
        (tags == tags2).Should().BeTrue();
        (tags.ToString() == tags2.ToString()).Should().BeTrue();

        var tags3 = new Tags();
        tags3.Set("key2,key1=value1");
        tags3.Count.Should().Be(2);
        (tags == tags3).Should().BeTrue();
        (tags.ToString() == tags3.ToString()).Should().BeTrue();

        var tags4 = new Tags("key2,key1=value1");
        tags4.Count.Should().Be(2);
        (tags == tags4).Should().BeTrue();
        (tags.ToString() == tags4.ToString()).Should().BeTrue();
    }

    [Fact]
    public void TagSerialization()
    {
        var tags = new Tags()
            .Set("key2")
            .Set("key1=value1");

        string json = tags.ToJson();

        Tags readTags = json.ToObject<Tags>().NotNull();
        readTags.Should().NotBeNull();

        (tags == readTags).Should().BeTrue();

        readTags.Has("key2").Should().BeTrue();
        readTags.Has("key1", "value1").Should().BeTrue();
        readTags.Has("key1", "fake").Should().BeFalse();
        readTags.Has("fake").Should().BeFalse();

        IReadOnlyDictionary<string, string?>? r2 = json.ToObject<IReadOnlyDictionary<string, string?>>();
        r2.Should().NotBeNull();
        r2!.ContainsKey("key2").Should().BeTrue();
        r2.ContainsKey("key1").Should().BeTrue();
        r2["key1"].Should().Be("value1");
    }

    internal enum ScheduleEdgeWorkState
    {
        None,
        Active,
        Completed,
        Failed
    }

    internal sealed record SimpleWorkTag
    {
        public ScheduleEdgeWorkState State { get; init; }
    }

    [Fact]
    public void TasSerializationWithStateObject()
    {
        var data = new SimpleWorkTag
        {
            State = ScheduleEdgeWorkState.Completed,
        };

        Tags tags = new Tags().Set(data);
        tags.Should().NotBeNull();
        tags.ToString().Should().Be("State=2");

        var readData = tags.ToObject<SimpleWorkTag>();
        (data == readData).Should().BeTrue();
    }


    internal sealed record HiddenWorkTag
    {
        [JsonIgnore] public ScheduleEdgeWorkState StateValue { get; init; }

        public string State
        {
            get => StateValue.ToString();
            init => StateValue = (ScheduleEdgeWorkState)Enum.Parse(typeof(ScheduleEdgeWorkState), value);
        }
    }

    [Fact]
    public void TasSerializationWithHiddenObject()
    {
        var data = new HiddenWorkTag
        {
            StateValue = ScheduleEdgeWorkState.Completed,
        };

        Tags tags = new Tags().Set(data);
        tags.Should().NotBeNull();
        tags.ToString().Should().Be("State=Completed");

        var readData = tags.ToObject<HiddenWorkTag>();
        (data == readData).Should().BeTrue();
    }

    internal sealed record ScheduleWorkTags
    {
        public bool Running { get; init; }
        public string Name { get; init; } = null!;
        public string StateName { get => State.ToString(); init => State = (ScheduleEdgeWorkState)Enum.Parse(typeof(ScheduleEdgeWorkState), StateName); }
        public ScheduleEdgeWorkState State { get; init; }
    }

    [Fact]
    public void TasSerializationWithObject()
    {
        var data = new ScheduleWorkTags
        {
            Running = true,
            Name = "name1",
            State = ScheduleEdgeWorkState.Completed,
        };

        Tags tags = new Tags().Set(data);
        tags.Should().NotBeNull();
        tags.ToString().Should().Be("Name=name1,Running=True,State=2,StateName=Completed");

        var readData = tags.ToObject<ScheduleWorkTags>();
        (data == readData).Should().BeTrue();
    }

    private record SimpleRecord
    {
        public string Name { get; init; } = null!;
        public int Value { get; init; }
    }

    private record SimpleRecord2
    {
        public string Name { get; init; } = null!;
        public string Country { get; init; } = null!;
        public decimal Amount { get; init; }
    }

    [Fact]
    public void TestRoundTripOfSingleClass()
    {
        var r1 = new SimpleRecord
        {
            Name = "name1",
            Value = 1,
        };

        Tags tags = new Tags().Set(r1);
        tags.ToString().Should().Be("Name=name1,Value=1");

        var r2 = tags.ToObject<SimpleRecord>();
        (r1 == r2).Should().BeTrue();
    }

    [Fact]
    public void TestRoundTripOfTwopClass()
    {
        var r1 = new SimpleRecord
        {
            Name = "name1",
            Value = 1,
        };

        var r2 = new SimpleRecord2
        {
            Name = "name1",
            Country = "country",
            Amount = 101.50m,
        };

        Tags tags = new Tags().Set(r1).Set(r2);
        tags.ToString().Should().Be("Amount=101.50,Country=country,Name=name1,Value=1");

        var r10 = tags.ToObject<SimpleRecord>();
        (r1 == r10).Should().BeTrue();

        var r11 = tags.ToObject<SimpleRecord2>();
        (r2 == r11).Should().BeTrue();
    }
}
