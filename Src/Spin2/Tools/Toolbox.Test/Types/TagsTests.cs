using System.Text.Json.Serialization;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class TagsTests
{
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

    [Fact]
    public void TagsSingleAndNotEqual()
    {
        var tags = new Tags().Set("key1");
        tags.Count.Should().Be(1);
        tags.ContainsKey("key1").Should().BeTrue();
        tags["key1"].Should().BeNull();
        tags.Has("key1").Should().BeTrue();
        tags.Has("key1", "value").Should().BeFalse();
        tags.Has("fake").Should().BeFalse();
        tags.Has("key1", "fake1").Should().BeFalse();
        tags.Has(null).Should().BeFalse();

        var tags2 = new Tags().Set("key1=value1");
        tags2.Count.Should().Be(1);
        tags2.ContainsKey("key1").Should().BeTrue();
        tags2["key1"].Should().Be("value1");
        tags2.Has("key1").Should().BeTrue();
        tags2.Has("key1", "value1").Should().BeTrue();
        tags2.Has("fake").Should().BeFalse();
        tags2.Has("key1", "fake1").Should().BeFalse();

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
        (tags.ToString() == tags2.ToString()).Should().BeFalse();
        (tags.ToString(true) == tags2.ToString(true)).Should().BeTrue();

        var tags3 = new Tags("key2=value2;key1=value1");
        (tags == tags3).Should().BeTrue();

        var tags4 = new Tags("key2=value2");
        (tags == tags4).Should().BeFalse();
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
        (tags.ToString(true) == tags2.ToString(true)).Should().BeTrue();

        var tags3 = new Tags();
        tags3.Set("key2=value2;key1=value1");
        tags3.Count.Should().Be(2);
        (tags == tags3).Should().BeTrue();
        (tags.ToString(true) == tags3.ToString(true)).Should().BeTrue();

        var tags4 = new Tags("key2=value2;key1=value1");
        tags4.Count.Should().Be(2);
        (tags == tags4).Should().BeTrue();
        (tags.ToString(true) == tags4.ToString(true)).Should().BeTrue();
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
        (tags.ToString(true) == tags2.ToString(true)).Should().BeTrue();

        var tags3 = new Tags();
        tags3.Set("key2;key1=value1");
        tags3.Count.Should().Be(2);
        (tags == tags3).Should().BeTrue();
        (tags.ToString(true) == tags3.ToString(true)).Should().BeTrue();

        var tags4 = new Tags("key2;key1=value1");
        tags4.Count.Should().Be(2);
        (tags == tags4).Should().BeTrue();
        (tags.ToString(true) == tags4.ToString(true)).Should().BeTrue();
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
        tags.ToString().Should().Be("Name=name1;Running=True;State=2;StateName=Completed");

        var readData = tags.ToObject<ScheduleWorkTags>();
        (data == readData).Should().BeTrue();
    }
}
