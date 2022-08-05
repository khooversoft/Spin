using Contract.sdk.Client;
using Contract.sdk.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Smart_Installment.sdk.Actor;
using Smart_Installment.sdk.Test.Application;
using Toolbox.Abstractions;
using Toolbox.Actor.Host;
using Toolbox.Extensions;
using Toolbox.Model;

namespace Smart_Installment.sdk.Test;

public class InstallmentContractActorTests
{
    [Fact]
    public async Task GivenContract_WhenCreatedAndAppended_ShouldPass()
    {
        IServiceProvider serviceProvider = await TestHost.Instance.GetServices();
        ContractClient client = serviceProvider.GetRequiredService<ContractClient>();

        DocumentId documentId = (DocumentId)"test/unit-tests-smart/contract4";

        var addPartyItems = Enumerable.Range(0, 5).Select(x => new PartyRecord
        {
            UserId = $"UserId_{x}",
            PartyType = $"PartyType_{x}",
            BankAccountId = $"AccountId_{x}",
        }).ToList();
        var partyFirst = 3;
        var partySecond = 2;
        var partyGetFirst = () => addPartyItems.Take(partyFirst);
        var partyGetSecond = () => addPartyItems.Skip(partyFirst).Take(partySecond);
        var partyGetThird = () => addPartyItems.Skip(partyFirst + partySecond);

        var addLedgerItems = Enumerable.Range(0, 8).Select(x => new LedgerRecord
        {
            Type = LedgerType.Credit,
            TrxType = $"TrxType_{x}",
            Amount = x * 150m,
        }).ToList();
        var ledgerFirst = 5;
        var ledgerSecond = 2;
        var ledgerGetFirst = () => addLedgerItems.Take(ledgerFirst);
        var ledgerGetSecond = () => addLedgerItems.Skip(ledgerFirst).Take(ledgerSecond);
        var ledgerGetThird = () => addLedgerItems.Skip(ledgerFirst + ledgerSecond);

        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-smart",
            Recursive = false,
        };

        IReadOnlyList<string> search = (await client.Search(query).ReadNext()).Records;
        if (search.Any(x => x == (string)documentId)) await client.Delete(documentId);


        IContractStoreActor actor = serviceProvider.GetActor<IContractStoreActor>(documentId.ToActorKey());

        var installmentHeader = new InstallmentHeader()
        {
            PrincipleId = "dev/user/endUser1@default.com",
            Name = "document4 name",
            DocumentId = documentId,
            Issuer = "dev/user/endUser1@default.com",
            Description = "test description for contract 4",
            NumPayments = 1,
            Principal = 10000.00m,
            Payment = 1000.00m,
            StartDate = DateTime.UtcNow,
        };

        await actor.Create(installmentHeader, CancellationToken.None);

        InstallmentContract installmentContract = (await actor.Get(CancellationToken.None)).NotNull();
        Test(installmentContract, Array.Empty<PartyRecord>(), Array.Empty<LedgerRecord>());


        // ========================================================================================

        partyGetFirst().ForEach(x => installmentContract.PartyRecords.Items.Add(x));
        ledgerGetFirst().ForEach(x => installmentContract.LedgerRecords.Items.Add(x));

        await actor.Append(installmentContract, CancellationToken.None);

        installmentContract = (await actor.Get(CancellationToken.None)).NotNull();
        Test(installmentContract, partyGetFirst(), ledgerGetFirst());


        // ========================================================================================

        partyGetSecond().ForEach(x => installmentContract.PartyRecords.Items.Add(x));
        ledgerGetSecond().ForEach(x => installmentContract.LedgerRecords.Items.Add(x));

        await actor.Append(installmentContract, CancellationToken.None);

        installmentContract = (await actor.Get(CancellationToken.None)).NotNull();
        Test(installmentContract, partyGetFirst().Concat(partyGetSecond()), ledgerGetFirst().Concat(ledgerGetSecond()));


        // ========================================================================================

        partyGetThird().ForEach(x => installmentContract.PartyRecords.Items.Add(x));
        ledgerGetThird().ForEach(x => installmentContract.LedgerRecords.Items.Add(x));

        await actor.Append(installmentContract, CancellationToken.None);

        installmentContract = (await actor.Get(CancellationToken.None)).NotNull();
        Test(installmentContract, addPartyItems, addLedgerItems);


        (await client.Delete(documentId)).Should().BeTrue();
    }

    private void Test(InstallmentContract contract, IEnumerable<PartyRecord> partyRecords, IEnumerable<LedgerRecord> ledgerRecords)
    {
        contract.Should().NotBeNull();
        contract.PartyRecords.Committed.Count.Should().Be(partyRecords.Count());
        contract.LedgerRecords.Committed.Count.Should().Be(ledgerRecords.Count());
        Enumerable.SequenceEqual(contract.PartyRecords.Committed, partyRecords).Should().BeTrue();
        Enumerable.SequenceEqual(contract.LedgerRecords.Committed, ledgerRecords).Should().BeTrue();
    }
}