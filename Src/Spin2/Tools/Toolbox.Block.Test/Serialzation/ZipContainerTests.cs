using System.IO.Compression;
using FluentAssertions;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Block.Signature;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Zip;

namespace Toolbox.Block.Test.Serialzation;

public class ZipContainerTests
{
    [Fact]
    public void GivenBlockChain_WhenContainerIsMemory_ShouldRoundTrip()
    {
        const string issuer = "user@domain.com";
        const string issuer2 = "user2@domain.com";
        const string zipPath = "$block";
        var date = DateTime.UtcNow;

        var issuerSignature = new PrincipalSignature(issuer, issuer, "userBusiness@domain.com");
        var issuerSignature2 = new PrincipalSignature(issuer2, issuer2, "userBusiness2@domain.com");

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(issuer)
            .Build()
            .Sign(x => issuerSignature);

        var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };
        var payload2 = new Payload2 { Last = "Last", Current = date, Author = "test" };

        blockChain.Add(payload, issuer);
        blockChain.Add(payload2, issuer);

        var getSignature = (string kid) =>
        {
            return kid switch
            {
                issuer => issuerSignature,
                issuer2 => issuerSignature2,
                _ => throw new ArgumentException($"Invalid kid={kid}"),
            };
        };

        blockChain = blockChain.Sign(getSignature);

        blockChain.Validate(getSignature);

        string blockChainHash = blockChain.ToMerkleTree().BuildTree().ToString();

        string json = blockChain.ToJson();

        using var writeBuffer = new MemoryStream();
        using (var writer = new ZipArchive(writeBuffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            writer.Write(zipPath, json);
        }

        writeBuffer.Length.Should().BeGreaterThan(0);
        writeBuffer.Seek(0, SeekOrigin.Begin);

        using var reader = new ZipArchive(writeBuffer, ZipArchiveMode.Read);
        string readJson = reader.ReadAsString(zipPath);

        BlockChain result = readJson.ToObject<BlockChainModel>()
            .NotNull(name: "Cannot deserialize")
            .ToBlockChain();

        result.Blocks.Count.Should().Be(3);

        result.Validate(getSignature);
        string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

        blockChainHash.Should().Be(resultChainHash);
    }

    private record Payload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public float Price { get; set; }
    }

    private record Payload2
    {
        public string? Last { get; set; }
        public DateTime Current { get; set; }
        public string? Author { get; set; }
    }
}
