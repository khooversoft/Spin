using System;
using FluentAssertions;
using Toolbox.Protocol;
using Xunit;

namespace Toolbox.Test.Abstract;

public class DocumentTests
{
    [Fact]
    public void GivenDocument_RoundTrip_ShouldPass()
    {
        DocumentId documentId = (DocumentId)"test/path";

        var rec1 = new Document
        {
            DocumentId = (string)documentId,
            ObjectClass = "objectClass",
            TypeName = "typeName",
            Content = "data",
        }.WithHash();

        rec1.IsHashVerify().Should().BeTrue();

        var rec2 = new Document
        {
            DocumentId = rec1.DocumentId,
            ObjectClass = rec1.ObjectClass,
            TypeName = rec1.TypeName,
            Hash = rec1.Hash,
            Content = rec1.Content,
            PrincipleId = rec1.PrincipleId,
        };

        rec2.IsHashVerify().Should().BeTrue();
        (rec1 == rec2).Should().BeTrue();
    }

    public sealed record RecordA : IEquatable<RecordA?>
    {
        public string Name { get; set; } = null!;

        public bool Equals(RecordA? other)
        {
            return other is not null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }

    [Fact]
    public void GivenDocument_WithAlteredData_WhenHashChecked_ShouldFail()
    {
        DocumentId documentId = (DocumentId)"test/path";

        var rec1 = new Document
        {
            DocumentId = (string)documentId,
            ObjectClass = "objectClass",
            TypeName = "typeName",
            Content = "data",
        }.WithHash();

        var rec2 = new Document
        {
            DocumentId = rec1.DocumentId,
            ObjectClass = rec1.ObjectClass,
            TypeName = "badType",
            Content = rec1.Content,
            PrincipleId = rec1.PrincipleId,
        }.WithHash() with
        {
            TypeName = rec1.TypeName,
        };

        rec2.IsHashVerify().Should().BeFalse();
        (rec1 == rec2).Should().BeFalse();
    }
}
