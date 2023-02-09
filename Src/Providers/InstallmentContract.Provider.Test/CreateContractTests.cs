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
using static System.Formats.Asn1.AsnWriter;

namespace InstallmentContract.Provider.Test;

public class CreateContractTests
{
    private const string _principleId = "user1@domain.com";
    private readonly PrincipalSignature _userSignature = new PrincipalSignature(_principleId, _principleId, "contractServices@domain.com");
    private readonly Dictionary<string, Document> _documentStore = new Dictionary<string, Document>();
    private readonly DocumentId _documentId = "test/providers/installment/contract1";

    [Fact]
    public async Task InstallmentContractFullLifeCycle()
    {
        IBlockDocumentStore store = GetStoreClient();
        ISigningClient signingClient = GetSigningClient();

        var service = new ContractService(store, signingClient, new NullLogger<ContractService>());

        await CreatingContract_ShouldPass(service);
        await TestBalance(service, - 1000.00m);
    }

    private async Task CreatingContract_ShouldPass(ContractService service)
    {
        var details = new ContractDetails
        {
            PrincipleId = _principleId,
            Name = "Installment contract for testing",
            DocumentId = _documentId,
            Issuer = "bank1@domain.com",
            Description = "Test installment contract 1",
            Initial = new InitialDetail
            {
                NumPayments = 10,
                PaymentAmount = 105.50m,
                FrequencyInDays = 7,
            },
            Ledgers = new LedgerRecord
            {
                Date = DateTime.UtcNow,
                Type = LedgerType.Debit,
                Amount = 1000.00m,
                Note = "Initial payout"
            }.ToEnumerable().ToArray(),
        }.Action(x => x.IsValid().Assert("Invalid payload"));

        NetMessage message = new NetMessageBuilder()
            .SetResourceUri("to")
            .SetCommand(CommandMethod.Create)
            .Add(PayloadBuilder.Create(details))
            .Build();

        NetResponse response = await service.Post(message, CancellationToken.None);

        response.Should().NotNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Message.Should().BeNullOrEmpty();

        Document document = _documentStore[(string)_documentId];
        document.Should().NotNull();
        document!.DocumentId.Should().Be((string)_documentId);
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

    private async Task TestBalance(ContractService service, decimal balance)
    {
        NetMessage message = new NetMessageBuilder()
            .SetResourceUri($"balance")
            .Add("documentId", (string)_documentId)
            .SetCommand(CommandMethod.Get)
            .Build();

        NetResponse response = await service.Post(message, CancellationToken.None);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Payloads.Count.Should().Be(1);

        BalanceRecord balanceRecord = response.Payloads.GetTypedPayloadSingle<BalanceRecord>().NotNull();
        balanceRecord.Should().NotBeNull();
        balanceRecord.Amount.Should().Be(balance);
    }


    private IBlockDocumentStore GetStoreClient() => new TestDocumentStore
    {
        ExistFunc = _ => true,
        SetFunc = x => true.Action(_ => _documentStore[x.DocumentId] = x),
        GetFunc = x => _documentStore[(string)x],
    };

    private ISigningClient GetSigningClient() => new TestSigningClient()
            .Add((_principleId, _userSignature));
}