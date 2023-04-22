using FluentAssertions;
using Toolbox.Block.Document;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace Toolbox.Block.Test.Container;

public class BlockDocumentCollectionStreamTests
{
    private const string _owner = "user@domain.com";
    private const string _issuer2 = "user2@domain.com";
    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private readonly PrincipalSignature _issuerSignature2 = new PrincipalSignature(_issuer2, _issuer2, "userBusiness2@domain.com");

    [Fact]
    public void GivenSingleDocument_ShouldPass()
    {
        var payloads = new[]
        {
            new Payload { Name = "Name1", Value = 1, Price = 1.5f },
            new Payload { Name = "Name2", Value = 2, Price = 2.5f },
            new Payload { Name = "Name2-offset", Value = 5, Price = 5.5f },
        };

        var doc = new BlockDocument(_owner)
            .Add(_ownerSignature)
            .Add(_issuerSignature2);

        BlockCollectionStream stream = doc.GetCollection("ledger");
        payloads.ForEach(x => stream.Add(x, _issuer2));

        doc.Sign();
        doc.Validate();

        string merkleTreeValue = doc.GetMerkleTreeValue();
        string json = doc.ToJson();

        BlockDocument doc2 = BlockDocument.Create(json);
        string doc2MerkleTreeValue = doc.GetMerkleTreeValue();
        merkleTreeValue.Should().Be(doc2MerkleTreeValue);

        Option<IReadOnlyList<Payload>> readPayloadOption = doc2.GetCollection("ledger").Get<Payload>();
        (readPayloadOption != default).Should().BeTrue();
        readPayloadOption.HasValue.Should().BeTrue();

        IReadOnlyList<Payload> payloads2 = readPayloadOption.Return();
        Enumerable.SequenceEqual(payloads, payloads2).Should().BeTrue();
    }

    [Fact]
    public void GivenSingleZipDocument_ShouldPass()
    {
        var payloads = new[]
        {
            new Payload { Name = "Name1", Value = 1, Price = 1.5f },
            new Payload { Name = "Name2", Value = 2, Price = 2.5f },
            new Payload { Name = "Name2-offset", Value = 5, Price = 5.5f },
        };

        var doc = new BlockDocument(_owner)
            .Add(_ownerSignature)
            .Add(_issuerSignature2);

        BlockCollectionStream stream = doc.GetCollection("ledger");
        payloads.ForEach(x => stream.Add(x, _issuer2));

        doc.Sign();
        doc.Validate();

        string merkleTreeValue = doc.GetMerkleTreeValue();

        byte[] zip = doc.ToZip();

        BlockDocument doc2 = BlockDocument.Create(zip);
        string doc2MerkleTreeValue = doc.GetMerkleTreeValue();
        merkleTreeValue.Should().Be(doc2MerkleTreeValue);

        Option<IReadOnlyList<Payload>> readPayloadOption = doc2.GetCollection("ledger").Get<Payload>();
        (readPayloadOption != default).Should().BeTrue();
        readPayloadOption.HasValue.Should().BeTrue();

        IReadOnlyList<Payload> payloads2 = readPayloadOption.Return();
        Enumerable.SequenceEqual(payloads, payloads2).Should().BeTrue();
    }

    private record Payload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public float Price { get; set; }
    }
}
