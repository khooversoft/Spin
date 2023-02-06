using System.Net;
using FluentAssertions;
using InstallmentContract.Provider.Models;
using InstallmentContract.Provider.Test.TestServices;
using Microsoft.Extensions.Logging.Abstractions;
using Provider.Abstractions;
using SpinNet.sdk.Application;
using SpinNet.sdk.Model;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Block.Signature;
using Toolbox.Extensions;
using Toolbox.Protocol;
using Toolbox.Security.Sign;
using Toolbox.Sign;
using Toolbox.Store;
using Toolbox.Tools;

namespace InstallmentContract.Provider.Test;

public class CreateContractTests
{
    private const string _principleId = "user1@domain.com";
    private static readonly PrincipalSignature _userSignature = new PrincipalSignature(_principleId, _principleId, "contractServices@domain.com");

    [Fact]
    public async Task CreatingContract_ShouldPass()
    {
        Document? document = null;

        IBlockDocumentStore store = new TestDocumentStore
        {
            ExistFunc = _ => true,
            SetFunc = x => true.Action(_ => document = x),
        };

        ISigningClient signingClient = new TestSigningClient()
            .Add((_principleId, _userSignature));

        var service = new ContractService(store, signingClient, new NullLogger<ContractService>());

        DocumentId documentId = "test/providers/installment/contract1";

         var details = new ContractDetails
        {
            PrincipleId = _principleId,
            Name = "Installment contract for testing",
            DocumentId = documentId,
            Issuer = "bank1@domain.com",
            Description = "Test installment contract 1",
            Initial = new InitialDetail(),
        }.Action(x => x.IsValid().Assert("Invalid payload"));

        NetMessage message = new NetMessageBuilder()
            .SetFromId("from")
            .SetToId("to")
            .SetCommand(CommandMethod.Create)
            .Add(PayloadBuilder.Create(details))
            .Build();

        NetResponse response = await service.Post(message, CancellationToken.None);

        response.Should().NotNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Message.Should().BeNullOrEmpty();

        document.Should().NotNull();
        document!.DocumentId.Should().Be((string)documentId);
        document.PrincipleId.Should().Be(_principleId);
        document.TypeName.Should().Be(typeof(BlockChain).GetTypeName());
        document.Content.Should().NotBeNullOrEmpty();

        var blockChain = document.Content.ToObject<BlockChainModel>()
            .NotNull()
            .ToBlockChain();

        blockChain.Should().NotBeNull();
        blockChain.Blocks.Count.Should().Be(2);
        blockChain!.IsValid().Should().BeTrue();
        blockChain.Validate(_ => _userSignature);

        blockChain.GetTypedBlocks<ContractDetails>().Count.Should().Be(1);
    }
}