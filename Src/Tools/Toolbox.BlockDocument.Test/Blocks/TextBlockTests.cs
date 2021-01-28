using FluentAssertions;
using System;
using Xunit;

namespace Toolbox.BlockDocument.Test.Blocks
{
    public class TextBlockTests
    {
        [Fact]
        public void GivenTextBlock_WhenConstructed_PropertiesShouldMatch()
        {
            TextBlock textBlock = new TextBlock("name", "contentType", "author", "content");

            textBlock.Should().NotBeNull();
            textBlock.Name.Should().Be("name");
            textBlock.ContentType.Should().Be("contentType");
            textBlock.Author.Should().Be("author");
            textBlock.Content.Should().Be("content");
        }

        [Fact]
        public void GivenTextBlock_WhenConstructedWithNulls_ShouldFail()
        {
            Action act = () => new TextBlock("", "contentType", "author", "content");
            act.Should().Throw<ArgumentException>();

            act = () => new TextBlock(null!, "contentType", "author", "content");
            act.Should().Throw<ArgumentException>();

            act = () => new TextBlock("name", "", "author", "content");
            act.Should().Throw<ArgumentException>();

            act = () => new TextBlock("name", "contentType", "", "content");
            act.Should().Throw<ArgumentException>();

            act = () => new TextBlock("name", "contentType", "author", null!);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenTextBlock_TestEqual_ShouldPass()
        {
            TextBlock v1 = new TextBlock("name", "contentType", "author", "content");

            TextBlock v2 = new TextBlock("name", "contentType", "author", "content");
            (v1 == v2).Should().BeTrue();
        }

        [Fact]
        public void GivenTextBlock_WhenInitialized_ShouldValidates()
        {
            var subject = new TextBlock("textName", "docx", "me", "This is a test of the blob - GivenTextBlock_WhenInitialized_ShouldValidates");
            subject.Digest.Should().NotBeNullOrEmpty();
            subject.Digest.Should().Be(subject.GetDigest());
        }

        [Fact]
        public void GivenTextBlock_WhenSameInitialized_ShouldValidate()
        {
            var subject = new TextBlock("textName", "docx", "me", "This is a test of the blob - GivenTextBlock_WhenSameInitialized_ShouldValidate");
            subject.Digest.Should().NotBeNullOrEmpty();

            var s1 = new TextBlock("textName", "docx", "me", "This is a test of the blob - GivenTextBlock_WhenSameInitialized_ShouldValidate");
            s1.Digest.Should().NotBeNullOrEmpty();

            subject.Digest.Should().Be(s1.Digest);
        }

        [Fact]
        public void GivenTextBlock_WhenCloned_ShouldValidate()
        {
            var subject = new TextBlock("textName", "docx", "me", "This is a test of the blob - GivenTextBlock_WhenCloned_ShouldValidate");
            subject.Digest.Should().NotBeNullOrEmpty();

            var s1 = new TextBlock(subject.Name, subject.ContentType, subject.Author, subject.Content);
            s1.Digest.Should().Be(s1.GetDigest());

            s1.Digest.Should().Be(subject.Digest);
            (subject == s1).Should().BeTrue();
        }
    }
}