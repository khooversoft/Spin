using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types.Maybe;

namespace Toolbox.Test.Types;

public class OptionStatusCodeTests
{
    [Fact]
    public void UsingValueTypeRequestStatus()
    {
        long v1 = 10;
        Option<long> optionV1 = v1.ToOption(StatusCode.BadRequest);
        optionV1.StatusCode.Should().Be(StatusCode.BadRequest);
        (optionV1 != default).Should().BeTrue();
        optionV1.HasValue.Should().BeTrue();
        optionV1.Value.Should().Be(v1);

        long? v2 = null;
        Option<long?> optionV2 = v2.ToOption(StatusCode.BadRequest);
        optionV2.StatusCode.Should().Be(StatusCode.BadRequest);
        (optionV2 == default).Should().BeFalse();
        optionV2.HasValue.Should().BeFalse();
        optionV2.Value.Should().Be(v2);
    }

    [Fact]
    public void UsingReferenceTypeRequestStatus()
    {
        string v1 = "this is it";
        Option<string> optionV1 = v1.ToOption(StatusCode.Created);
        optionV1.StatusCode.Should().Be(StatusCode.Created);
        (optionV1 != default).Should().BeTrue();
        optionV1.HasValue.Should().BeTrue();
        optionV1.Value.Should().Be(v1);

        string? v2 = null;
        Option<string> optionV2 = v2.ToOption(StatusCode.Created);
        optionV2.StatusCode.Should().Be(StatusCode.Created);
        (optionV2 == default).Should().BeFalse();
        optionV2.HasValue.Should().BeFalse();
        optionV2.Value.Should().Be(v2);
    }
}
