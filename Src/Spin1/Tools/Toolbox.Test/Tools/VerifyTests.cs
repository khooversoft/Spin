using System;
using System.Collections.Generic;
using FluentAssertions;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools;

public class VerifyTests
{
    [Fact]
    public void GivenNotNull_VerifyNotNull_ShouldPass()
    {
        List<string>? kv = new List<string>();
        kv.NotNull();
    }

    [Fact]
    public void GivenNotNull_VerifyNull_ShouldPass()
    {
        List<string>? kv = null;

        kv = null;

        Action action = () => kv.NotNull();
        action.Should().Throw<ArgumentNullException>();
    }
}
