using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class StructureLineBuilderTests
{
    [Fact]
    public void Empty()
    {
        Verify.Throw<ArgumentException>(() =>
        {
            new StructureLineBuilder().Build();
        });
    }

    [Fact]
    public void MessageOnly()
    {
        var result = new StructureLineBuilder()
            .Add("test")
            .Build();

        result.Message.Be("test");
        result.Args.Length.Be(0);

        result.GetVariables().Count.Be(0);
        result.BuildStringFormat().Be("test");
    }

    [Fact]
    public void OptionOk()
    {
        Option option = StatusCode.OK;

        var result = new StructureLineBuilder()
            .Add(option)
            .Build();

        result.Message.Be("statusCode={statusCode}, error={error}");
        result.Args.Action(x =>
        {
            // TODO
            x.Length.Be(2);
            x[0].NotNull().Cast<StatusCode>().Be(StatusCode.OK);
            x[1].NotNull().Cast<string>().Be("< no error >");
        });

        result.GetVariables().Action(x =>
        {
            x.Count.Be(2);
            x[0].Be("statusCode");
            x[1].Be("error");
        });

        result.GetVariables().SequenceEqual(["statusCode", "error"]).BeTrue();
        result.GetVariables().BeEquivalent(["error", "statusCode"]);
        result.BuildStringFormat().Be("statusCode={0}, error={1}");
        result.Format().Be("statusCode=OK, error=< no error >");
    }

    [Fact]
    public void SingleBuilder()
    {
        var result = new StructureLineBuilder()
            .Add("value={value}", 1)
            .Build();

        result.Message.Be("value={value}");
        result.Args.Action(x =>
        {
            x.Length.Be(1);
            x[0].NotNull().Cast<int>().Be(1);
        });

        result.GetVariables().SequenceEqual(["value"]).BeTrue();
        result.BuildStringFormat().Be("value={0}");
        result.Format().Be("value=1");
    }

    [Fact]
    public void MultiArrayCompact()
    {
        var result = new StructureLineBuilder()
            .Add("value={value}, count={count}", 1, 2)
            .Build();

        result.Message.Be("value={value}, count={count}");
        result.Args.Action(x =>
        {
            x.Length.Be(2);
            x[0].NotNull().Cast<int>().Be(1);
            x[1].NotNull().Cast<int>().Be(2);
        });
    }

    [Fact]
    public void MultiArray()
    {
        var result = new StructureLineBuilder()
            .Add("value={value}", 1)
            .Add("count={count}", 2)
            .Build();

        result.Message.Be("value={value}, count={count}");
        result.Args.Action(x =>
        {
            x.Length.Be(2);
            x[0].NotNull().Cast<int>().Be(1);
            x[1].NotNull().Cast<int>().Be(2);
        });
    }

    [Fact]
    public void CompactAndMultiArray()
    {
        var result = new StructureLineBuilder()
            .Add("value={value}", 1)
            .Add("count={count}", 2)
            .Add("first={firstValue}, secondCount={secondCount}", 3, 4)
            .Build();

        result.Message.Be("value={value}, count={count}, first={firstValue}, secondCount={secondCount}");
        result.Args.Action(x =>
        {
            x.Length.Be(4);
            x[0].NotNull().Cast<int>().Be(1);
            x[1].NotNull().Cast<int>().Be(2);
            x[2].NotNull().Cast<int>().Be(3);
            x[3].NotNull().Cast<int>().Be(4);
        });
    }

    [Fact]
    public void Append()
    {
        string fmt = "this is a test";
        object?[] objects = new object[0];

        var result = new StructureLineBuilder()
            .Add(fmt, objects)
            .Add("traceId={traceId}", "this.trace.id")
            .Add("statusCode={statusCode}, Error={error}", StatusCode.Conflict, "error")
            .Build();

        result.Message.Be(fmt + ", traceId={traceId}, statusCode={statusCode}, Error={error}");
        result.Args.Action(x =>
        {
            x.Length.Be(3);
            x[0].NotNull().Cast<string>().Be("this.trace.id");
            x[1].NotNull().Cast<StatusCode>().Be(StatusCode.Conflict);
            x[2].NotNull().Cast<string>().Be("error");
        });
    }


    [Fact]
    public void AppendWithNullArgs()
    {
        string fmt = "this is a test";

        var result = new StructureLineBuilder()
            .Add(fmt, null)
            .Add("traceId={traceId}", "this.trace.id")
            .Add("statusCode={statusCode}, Error={error}", StatusCode.Conflict, "error")
            .Build();

        result.Message.Be(fmt + ", traceId={traceId}, statusCode={statusCode}, Error={error}");
        result.Args.Action(x =>
        {
            x.Length.Be(3);
            x[0].NotNull().Cast<string>().Be("this.trace.id");
            x[1].NotNull().Cast<StatusCode>().Be(StatusCode.Conflict);
            x[2].NotNull().Cast<string>().Be("error");
        });
    }

    [Fact]
    public void AppendWithBaseValues()
    {
        string fmt = "this is a test, value={value}, count={count}";
        object?[] objects = new object[] { 1, 2 };

        var result = new StructureLineBuilder()
            .Add(fmt, objects)
            .Add("traceId={traceId}", "this.trace.id")
            .Add("statusCode={statusCode}, Error={error}", StatusCode.Conflict, "error")
            .Build();

        result.Message.Be(fmt + ", traceId={traceId}, statusCode={statusCode}, Error={error}");
        result.Args.Action(x =>
        {
            x.Length.Be(5);
            x[0].NotNull().Cast<int>().Be(1);
            x[1].NotNull().Cast<int>().Be(2);
            x[2].NotNull().Cast<string>().Be("this.trace.id");
            x[3].NotNull().Cast<StatusCode>().Be(StatusCode.Conflict);
            x[4].NotNull().Cast<string>().Be("error");
        });
    }

    [Fact]
    public void UnbalancedShouldFailOnBuild()
    {
        Verify.Throw<ArgumentException>(() =>
        {
            var result = new StructureLineBuilder()
                .Add("no variable", "oops")
                .Add("traceId={traceId}", "this.trace.id")
                .Add("statusCode={statusCode}, Error={error}", StatusCode.Conflict, "error")
                .Build();
        });

        Verify.Throw<ArgumentException>(() =>
        {
            var result = new StructureLineBuilder()
                .Add("traceId={traceId}", "this.trace.id", 1)
                .Add("statusCode={statusCode}, Error={error}", StatusCode.Conflict, "error")
                .Build();
        });
    }
}
