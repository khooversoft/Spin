using Contract.sdk.Client;
using Contract.sdk.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Smart_Installment.sdk.Actor;
using Smart_Installment.sdk.Test.Application;
using Toolbox.Abstractions;
using Toolbox.Actor.Host;
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

        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-smart",
            Recursive = false,
        };

        IReadOnlyList<string> search = (await client.Search(query).ReadNext()).Records;
        if (search.Any(x => x == (string)documentId)) await client.Delete(documentId);


        IInstallmentContractActor actor = serviceProvider.GetActor<IInstallmentContractActor>(documentId.ToActorKey());

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

        
        await actor.CreateContract(installmentHeader, CancellationToken.None);

        InstallmentContract installmentContract = (await actor.Get(CancellationToken.None)).NotNull();
        installmentContract.Should().NotBeNull();

        var parties = Enumerable.Range(0, 2).Select(x => new PartyRecord
        {
            UserId = $"UserId_{x}",
            PartyType = $"PartyType_{x}",
            BankAccountId = $"AccountId_{x}",
        }).ToList();

        parties.ForEach(x => installmentContract.Parties.Add(x));

        var ledgers = Enumerable.Range(0, 5).Select(x => new LedgerRecord
        {
            Type = LedgerType.Credit,
            TrxType = $"TrxType_{x}",
            Amount = x * 150m,
        }).ToList();

        ledgers.ForEach(x => installmentContract.Ledger.Add(x));

        await actor.Append(installmentContract, CancellationToken.None);


        installmentContract = (await actor.Get(CancellationToken.None)).NotNull();
        installmentContract.Should().NotBeNull();

        installmentContract.Parties.Add(new PartyRecord
        {
            UserId = "UserId_new",
            PartyType = "PartyType_new",
            BankAccountId = $"AccountId_new",
        });

        installmentContract.Ledger.Add(new LedgerRecord
        {
            Type = LedgerType.Credit,
            TrxType = $"TrxType_new",
            Amount = 2150m,
        });

        await actor.Append(installmentContract, CancellationToken.None);


        InstallmentContract? readInstallmentContract = await actor.Get(CancellationToken.None);
        readInstallmentContract.Should().NotBeNull();

        (installmentContract == readInstallmentContract).Should().BeTrue();

        (await client.Delete(documentId)).Should().BeTrue();
    }
}