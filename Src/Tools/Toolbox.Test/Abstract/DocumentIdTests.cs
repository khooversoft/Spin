using FluentAssertions;
using System;
using Toolbox.Abstractions.Protocol;
using Xunit;

namespace Toolbox.Test.Abstract;

public class DocumentIdTests
{
    [Theory]
    [InlineData("domain/a/a")]
    [InlineData("domain/ab/ab")]
    [InlineData("domain/a.1/a")]
    [InlineData("domain/a.a/a")]
    [InlineData("domain/a-a/a")]
    [InlineData("domain/user@domain.com/a")]
    [InlineData("domain/a/a.b")]
    [InlineData("domain/A/A.b")]
    [InlineData("domain/service/1b")]
    [InlineData("domain/service/1")]
    [InlineData("domain/service/A/1A.b")]
    [InlineData("domain/service/A/A.b/3c2/b3")]
    [InlineData("domain/A/A.b/c2/b3")]
    [InlineData("1")]
    [InlineData("domain/1")]
    [InlineData("domain")]
    [InlineData("container:domain")]
    [InlineData("container:domain/service")]
    [InlineData("container:domain/service/path")]
    public void GivenValidArticleId_WhenVerified_ShouldPass(string id)
   {
        ((DocumentId)id).Id.Should().Be(id.ToLower());
        ((string)(DocumentId)id).Should().Be(id.ToLower());
        ((string)new DocumentId(id)).Should().Be(id.ToLower());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(":")]
    [InlineData("/")]
    [InlineData("domain///")]
    [InlineData("domain:")]
    [InlineData("domain/service//")]
    [InlineData("domain/service/-")]
    [InlineData("domain/service/.")]
    [InlineData("domain/.")]
    [InlineData("domain/service/*")]
    [InlineData("domain/service/1/")]
    [InlineData("domain/service/1.")]
    public void GivenBadArticleId_WhenVerified_ShouldFail(string id)
    {
        Action action = () => _ = (DocumentId)id;
        action.Should().Throw<ArgumentException>();

        action = () => _ = (DocumentId)id;
        action.Should().Throw<ArgumentException>();

        action = () => _ = new DocumentId(id);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenTwoDocumentID_WithSameValue_ShouldEqual()
    {
        var document1 = (DocumentId)"test/path";
        var document2 = (DocumentId)"test/path";

        (document1 == document2).Should().BeTrue();
        (document1 != document2).Should().BeFalse();
    }

    [Fact]
    public void GivenTwoDocumentID_WithDifferentValue_ShouldNotEqual()
    {
        var document1 = (DocumentId)"test/path";
        var document2 = (DocumentId)"test/path2";

        (document1 == document2).Should().BeFalse();
        (document1 != document2).Should().BeTrue();
    }
}
