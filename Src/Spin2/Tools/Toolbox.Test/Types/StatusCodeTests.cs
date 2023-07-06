using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class StatusCodeTests
{
    [Fact]
    public void UsingValueTypeRequestStatus()
    {
        long v1 = 10;
        Option<long> optionV1 = new Option<long>(v1, StatusCode.BadRequest);
        optionV1.StatusCode.Should().Be(StatusCode.BadRequest);
        (optionV1 != default).Should().BeTrue();
        optionV1.HasValue.Should().BeTrue();
        optionV1.Value.Should().Be(v1);

        long? v2 = null;
        Option<long?> optionV2 = new Option<long?>(v2, StatusCode.BadRequest);
        optionV2.StatusCode.Should().Be(StatusCode.BadRequest);
        (optionV2 == default).Should().BeFalse();
        optionV2.HasValue.Should().BeFalse();
        optionV2.Value.Should().Be(v2);
    }

    [Fact]
    public void UsingReferenceTypeRequestStatus()
    {
        string v1 = "this is it";
        Option<string> optionV1 = new Option<string>(v1, StatusCode.Created);
        optionV1.StatusCode.Should().Be(StatusCode.Created);
        (optionV1 != default).Should().BeTrue();
        optionV1.HasValue.Should().BeTrue();
        optionV1.Value.Should().Be(v1);

        string? v2 = null;
        Option<string?> optionV2 = new Option<string?>(v2, StatusCode.Created);
        optionV2.StatusCode.Should().Be(StatusCode.Created);
        (optionV2 == default).Should().BeFalse();
        optionV2.HasValue.Should().BeFalse();
        optionV2.Value.Should().Be(v2);
    }
}
