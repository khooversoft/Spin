using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class StructureLineBuilderTests
{
    [Fact]
    public void MessageOnly()
    {
        var result = StructureLineBuilder.Start()
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

        var result = StructureLineBuilder.Start()
            .Add(option)
            .Build();

        result.Message.Be("statusCode={statusCode}");
        result.Args.Action(x =>
        {
            x.Length.Be(1);
            x[0].NotNull().Cast<StatusCode>().Be(StatusCode.OK);
        });

        result.GetVariables().Action(x =>
        {
            x.Count.Be(1);
            x[0].Be("statusCode");
        });

        result.GetVariables().SequenceEqual(["statusCode"]).BeTrue();
        result.GetVariables().BeEquivalent(["statusCode"]);
        result.BuildStringFormat().Be("statusCode={0}");
        result.Format().Be("statusCode=OK");
    }

    [Fact]
    public void SingleBuilder()
    {
        var result = StructureLineBuilder.Start()
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
        var result = StructureLineBuilder.Start()
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
        var result = StructureLineBuilder.Start()
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
        var result = StructureLineBuilder.Start()
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

        var result = StructureLineBuilder.Start()
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

        var result = StructureLineBuilder.Start()
            .Add(fmt, null!)
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
    public void AppendWithEmptyArgs()
    {
        string fmt = "this is a test";

        var result = StructureLineBuilder.Start()
            .Add(fmt, [])
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

        var result = StructureLineBuilder.Start()
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
            var result = StructureLineBuilder.Start()
                .Add("no variable", "oops")
                .Add("traceId={traceId}", "this.trace.id")
                .Add("statusCode={statusCode}, Error={error}", StatusCode.Conflict, "error")
                .Build();
        });

        Verify.Throw<ArgumentException>(() =>
        {
            var result = StructureLineBuilder.Start()
                .Add("traceId={traceId}", "this.trace.id", 1)
                .Add("statusCode={statusCode}, Error={error}", StatusCode.Conflict, "error")
                .Build();
        });
    }

    [Fact]
    public void EmptyMessageAndNullAreIgnored_NoLeadingComma()
    {
        var result = StructureLineBuilder.Start()
            .Add("")          // no-op
            .Add(null)        // no-op
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
    public void NullValueForPlaceholderShouldFailOnBuild()
    {
        Verify.Throw<ArgumentException>(() =>
        {
            var _ = StructureLineBuilder.Start()
                .Add("value={value}", (object?)null)  // placeholder exists, arg is null => unbalanced
                .Build();
        });
    }

    [Fact]
    public void VariableOrderIsPreservedAcrossSegments()
    {
        var result = StructureLineBuilder.Start()
            .Add("a={a}", 1)
            .Add("b={b}, c={c}", 2, 3)
            .Build();

        result.Message.Be("a={a}, b={b}, c={c}");
        var vars = result.GetVariables();

        vars.SequenceEqual(["a", "b", "c"]).BeTrue();
        vars.Count.Be(3);

        result.Args.Action(x =>
        {
            x.Length.Be(3);
            x[0].NotNull().Cast<int>().Be(1);
            x[1].NotNull().Cast<int>().Be(2);
            x[2].NotNull().Cast<int>().Be(3);
        });

        result.BuildStringFormat().Be("a={0}, b={1}, c={2}");
        result.Format().Be("a=1, b=2, c=3");
    }

    [Fact]
    public void AppendMessageOnlyThenMore_UsesMessageOnlyOverload()
    {
        string fmt = "this is a test";

        var result = StructureLineBuilder.Start()
            .Add(fmt) // message-only overload (no array allocation)
            .Add("traceId={traceId}", "this.trace.id")
            .Build();

        result.Message.Be(fmt + ", traceId={traceId}");
        result.Args.Action(x =>
        {
            x.Length.Be(1);
            x[0].NotNull().Cast<string>().Be("this.trace.id");
        });

        result.GetVariables().SequenceEqual(["traceId"]).BeTrue();
        result.BuildStringFormat().Be("this is a test, traceId={0}");
        result.Format().Be("this is a test, traceId=this.trace.id");
    }
}
