using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools.Local;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class LocalProcessBuilderTests
{

    [Theory]
    [InlineData("smartc.exe", "smartc.exe", null, null)]
    [InlineData(@"c:\smartc.exe", @"c:\smartc.exe", null, @"c:\")]
    [InlineData(@"c:\folder\smartc.exe", @"c:\folder\smartc.exe", null, @"c:\folder")]
    [InlineData(@"folder\smartc.exe", @"folder\smartc.exe", null, "folder")]
    [InlineData(@"c:\folder\folder2\smartc.exe", @"c:\folder\folder2\smartc.exe", null, @"c:\folder\folder2")]

    [InlineData("smartc.exe run", "smartc.exe", "run", null)]
    [InlineData("smartc.exe run second", "smartc.exe", "run second", null)]
    [InlineData(@"c:\smartc.exe run", @"c:\smartc.exe", "run", @"c:\")]
    [InlineData(@"c:\folder\smartc.exe run", @"c:\folder\smartc.exe", "run", @"c:\folder")]
    [InlineData(@"folder\smartc.exe run", @"folder\smartc.exe", "run", "folder")]
    [InlineData(@"c:\folder\folder2\smartc.exe run", @"c:\folder\folder2\smartc.exe", "run", @"c:\folder\folder2")]
    [InlineData(@"c:\folder\folder2\smartc.exe run second", @"c:\folder\folder2\smartc.exe", "run second", @"c:\folder\folder2")]
    public void TestCommandLineParser2(string cmdLine, string exeFile, string? args, string? workingDirectory)
    {
        var builder = new LocalProcessBuilder().SetCommandLine(cmdLine);

        builder.ExecuteFile.Should().Be(exeFile);
        builder.Arguments.Should().Be(args);
        builder.WorkingDirectory.Should().Be(workingDirectory);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void TestCommandLineParserFailure2(string cmdLine)
    {
        Action act = () => new LocalProcessBuilder().SetCommandLine(cmdLine);
        act.Should().Throw<ArgumentException>();
    }
}
