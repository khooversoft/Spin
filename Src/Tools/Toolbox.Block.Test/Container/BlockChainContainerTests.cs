using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Toolbox.Block.Application;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Block.Signature;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Toolbox.Block.Test.Container;

public class BlockChainContainerTests
{
    private const string _issuer = "user@domain.com";
    private const string _issuer2 = "user2@domain.com";
    private readonly PrincipalSignature _issuerSignature = new PrincipalSignature(_issuer, _issuer, "userBusiness@domain.com");
    private readonly PrincipalSignature _issuerSignature2 = new PrincipalSignature(_issuer2, _issuer2, "userBusiness2@domain.com");
    private readonly DateTime _date = DateTime.UtcNow;

    [Fact]
    public void GivenBlockChain_WhenContainered_ShouldRoundTrip()
    {
        SerializeProcess();
    }

    [Fact]
    public void FindBlockBaseOnType()
    {
        BlockChain blockChain = SerializeProcess();

        blockChain.Blocks.Should().HaveCount(4);

        IReadOnlyList<Payload> payloads = blockChain.GetTypedBlocks<Payload>();
        payloads.Should().HaveCount(2);

        new Payload[]
        {
            new Payload { Name = "Name1", Value = 2, Price = 10.5f },
            new Payload { Name = "Name2", Value = 3, Price = 20.5f },
        }
        .Action(x => Enumerable.SequenceEqual(payloads, x));

        IReadOnlyList<Payload2> payload2s = blockChain.GetTypedBlocks<Payload2>();
        payload2s.Should().HaveCount(1);

        new Payload2[]
        {
            new Payload2 { Last = "Last", Current = _date, Author = "test" }
        }
        .Action(x => Enumerable.SequenceEqual(payload2s, x));
    }


    private BlockChain CreateChain()
    {

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(_issuer)
            .Build()
            .Sign(x => _issuerSignature);

        var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };
        var payload2 = new Payload2 { Last = "Last", Current = _date, Author = "test" };
        var payload3 = new Payload { Name = "Name2", Value = 3, Price = 20.5f };

        blockChain.Add(payload, _issuer);
        blockChain.Add(payload2, _issuer);
        blockChain.Add(payload3, _issuer);

        blockChain = blockChain.Sign(GetSignature);
        blockChain.Validate(GetSignature);
        return blockChain;
    }

    private BlockChain SerializeProcess()
    {
        BlockChain blockChain = CreateChain();
        string blockChainHash = blockChain.ToMerkleTree().BuildTree().ToString();

        byte[] blockChainData = blockChain
            .ToBlockChainModel()
            .ToPackage();

        var readModel = blockChainData.ToBlockChainModel();

        BlockChain result = readModel.ToBlockChain();

        result.Validate(GetSignature);
        string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

        blockChainHash.Should().Be(resultChainHash);

        return result;
    }

    private PrincipalSignature GetSignature(string kid) => kid switch
    {
        _issuer => _issuerSignature,
        _issuer2 => _issuerSignature2,
        _ => throw new ArgumentException($"Invalid kid={kid}"),
    };

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
