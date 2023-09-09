using FluentAssertions;
using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class ArgumentToolTests
{
    [Fact]
    public void TestCommandLineArguments()
    {
        string[] args = new string[]
        {
            "run",
            "file",
            "--argument",
            "value",
        };

        (string[] ConfigArgs, string[] CommandLineArgs) result = ArgumentTool.Split(args);
        result.Should().NotBeNull();
        result.ConfigArgs.Length.Should().Be(0);
        result.CommandLineArgs.Length.Should().Be(4);

        Enumerable.SequenceEqual(args, result.CommandLineArgs).Should().BeTrue();
    }

    [Fact]
    public void TestConfigurationArguments()
    {
        string[] args = new string[]
        {
            "value=1",
            "value:sub=2",
        };

        (string[] ConfigArgs, string[] CommandLineArgs) result = ArgumentTool.Split(args);
        result.Should().NotBeNull();
        result.ConfigArgs.Length.Should().Be(2);
        result.CommandLineArgs.Length.Should().Be(0);

        Enumerable.SequenceEqual(args, result.ConfigArgs).Should().BeTrue();
    }

    [Fact]
    public void TestMixConfigurationArguments()
    {
        string[] args = new string[]
        {
            "run",
            "file",
            "value=1",
            "value:sub=2",
            "--argument",
            "value",
        };

        (string[] ConfigArgs, string[] CommandLineArgs) result = ArgumentTool.Split(args);
        result.Should().NotBeNull();
        result.ConfigArgs.Length.Should().Be(2);
        result.CommandLineArgs.Length.Should().Be(4);

        Enumerable.SequenceEqual(args.Skip(2).Take(2), result.ConfigArgs).Should().BeTrue();
        Enumerable.SequenceEqual(args.Take(2).Concat(args.Skip(4)), result.CommandLineArgs).Should().BeTrue();
    }
}
