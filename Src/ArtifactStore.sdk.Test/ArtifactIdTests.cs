using ArtifactStore.sdk.Model;
using FluentAssertions;
using System;
using Xunit;

namespace ArtifactStore.sdk.Test
{
    public class ArtifactIdTests
    {
        [Theory]
        [InlineData("a/a")]
        [InlineData("ab/ab")]
        [InlineData("a.1/a")]
        [InlineData("a.a/a")]
        [InlineData("a-a/a")]
        [InlineData("a/a.b")]
        [InlineData("A/A.b")]
        [InlineData("A/A.b/c2/b3")]
        public void GivenValidArticleId_WhenVerified_ShouldPass(string id)
        {
            _ = ((ArtifactId)id).Id.Should().Be(id.ToLower());
            _ = ((string)(ArtifactId)id).Should().Be(id.ToLower());
            _ = ((string)(new ArtifactId(id))).Should().Be(id.ToLower());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("/")]
        [InlineData("-")]
        [InlineData(".")]
        [InlineData("1b")]
        [InlineData("*")]
        [InlineData("1/")]
        [InlineData("1.")]
        [InlineData("A/1A.b")]
        [InlineData("A/A.b/3c2/b3")]
        public void GivenBadArticleId_WhenVerified_ShouldFail(string id)
        {
            Action action = () => _ = (ArtifactId)id;
            action.Should().Throw<ArgumentException>();

            action = () => _ = (string)(ArtifactId)id;
            action.Should().Throw<ArgumentException>();

            action = () => _ = (string)(new ArtifactId(id));
            action.Should().Throw<ArgumentException>();
        }
    }
}