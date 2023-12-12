using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types;

namespace NBlog.sdk.test;

public class FileIdTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("a1")]
    [InlineData("a1-a")]
    [InlineData("a1.a")]
    [InlineData("a1.ext")]
    [InlineData("file/a1.ext")]
    [InlineData("a1/b")]
    [InlineData("a1/b/c")]
    [InlineData("a1/b/c2")]
    public void ValidFileIds(string value)
    {
        var option = FileId.Create(value);
        option.IsOk().Should().BeTrue(option.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1a")]
    [InlineData("1a//b")]
    [InlineData("1a/name/..dkdk")]
    [InlineData("1a1/b/c2")]
    [InlineData("a1/2b/c2")]
    [InlineData("a1/b/3c2")]
    [InlineData("a1./b/c2")]
    [InlineData("a1/b./c2")]
    public void InvalidFileIds(string? value)
    {
        var option = FileId.Create(value!);
        option.IsError().Should().BeTrue(option.ToString());
    }
}
