using FluentAssertions;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Types;
using Xunit;

namespace Toolbox.BlockDocument.Test.Blocks
{
    public class BlobBlockTests
    {
        [Fact]
        public void GivenBlobBlock_WhenInitialized_ShouldValidates()
        {
            IReadOnlyList<byte> payload = "This is a test of the blob - GivenBlobBlock_WhenInitialized_ShouldValidates".ToBytes();

            var subject = new BlobBlock("blockName", "docx", "me", payload);
            subject.Digest.Should().NotBeNullOrEmpty();

            subject.Digest.Should().Be(subject.GetDigest());
        }

        [Fact]
        public void GivenBlobBlock_WhenSameInitialized_ShouldValidate()
        {
            UnixDate now = UnixDate.UtcNow;

            IReadOnlyList<byte> payload = "This is a test of the blob - GivenHeaderBlock_WhenSameInitialized_ShouldValidate".ToBytes();

            var subject = new BlobBlock("blockName", "docx", "me", payload);
            subject.Digest.Should().NotBeNullOrEmpty();

            var s1 = new BlobBlock("blockName", "docx", "me", payload);
            s1.Digest.Should().NotBeNullOrEmpty();

            subject.Digest.Should().Be(s1.Digest);
        }

        [Fact]
        public void GivenBlobBlock_WhenCloned_ShouldValidate()
        {
            UnixDate now = UnixDate.UtcNow;
            IReadOnlyList<byte> payload = "This is a test of the blob - GivenHeaderBlock_WhenCloned_ShouldValidate".ToBytes();

            var subject = new BlobBlock("blockName1", "xlsx", "you", payload);
            subject.Digest.Should().NotBeNullOrEmpty();

            var s1 = new BlobBlock(subject.Name, subject.ContentType, subject.Author, subject.Content);
            s1.Digest.Should().Be(s1.GetDigest());

            s1.Digest.Should().Be(subject.Digest);
            (subject == s1).Should().BeTrue();
        }
    }
}