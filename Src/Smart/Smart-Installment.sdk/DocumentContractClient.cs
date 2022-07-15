using Contract.sdk.Client;
using Contract.sdk.Models;
using ContractHost.sdk.Host;
using ContractHost.sdk.Model;
using Microsoft.Extensions.Logging;
using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Smart_Installment.sdk;

public class DocumentContractClient
{
    private readonly ContractClient _client;
    private readonly ILogger<DocumentContractClient> _logger;
    private readonly ContractHostOption _contractHostOption;

    public DocumentContractClient(ContractClient client, ContractHostOption contractHostOption, ILogger<DocumentContractClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
        _contractHostOption = contractHostOption.Verify();
    }

    public async Task Create(InstallmentContract installmentContract, CancellationToken token = default)
    {
        installmentContract.Verify();

        var blocks = new List<DataBlock>();

        var details = new ContractDetails
        {
            NumPayments = installmentContract.NumPayments,
            Principal = installmentContract.Principal,
            Payment = installmentContract.Payment,
            StartDate = installmentContract.StartDate,
            CompletedDate = installmentContract.CompletedDate,
        };
        blocks.Add(details.ToDataBlock(_contractHostOption.PrincipleId));

        if (installmentContract.Parties.Count > 0)
        {
            blocks.Add(installmentContract.Parties.ToDataBlock(_contractHostOption.PrincipleId));
        }

        if (installmentContract.Ledger.Count > 0)
        {
            blocks.Add(installmentContract.Ledger.ToDataBlock(_contractHostOption.PrincipleId));
        }

        //var header = new BlkHeader
        //{
        //    PrincipleId = _contractHostOption.PrincipleId,
        //    DocumentId = _contractHostOption.DocumentId,
        //    Creator = installmentContract.Creator,
        //    Description = installmentContract.Description,
        //    Blocks = blocks,
        //    Properties = installmentContract.Properties?.ToList(),
        //};

        //_logger.LogInformation("Creating contract id={id}", _contractHostOption.DocumentId);
        //await _client.Create(header, token);
    }

    public async Task Append<T>(IEnumerable<T> items, CancellationToken token = default)
    {
        items.NotNull();
        items.Assert(x => x.Any(), "Empty list");

        var collection = new BlkCollection
        {
            PrincipleId = _contractHostOption.PrincipleId,
            Blocks = items
                .ToDataBlock(_contractHostOption.PrincipleId)
                .ToEnumerable()
                .ToList(),
        };

        _logger.LogInformation("Appending items to contract id={id}", _contractHostOption.DocumentId);
        await _client.Append((DocumentId)_contractHostOption.DocumentId, collection, token);
    }

    public async Task<InstallmentContract> GetContract(CancellationToken token = default)
    {
        BlockChainModel blockChainModel = await _client.Get((DocumentId)_contractHostOption.DocumentId, token);

        BlkHeader header = blockChainModel
            .GetBlockType<BlkHeader>()
            .Assert(x => x.Count > 0, "Header not found")
            .Last();

        ContractDetails details = blockChainModel
            .GetBlockType<ContractDetails>()
            .Assert(x => x.Count > 0, "Contract detail not found")
            .Last();

        IReadOnlyList<PartyRecord> parties = blockChainModel
            .GetBlockType<PartyRecord>();

        IReadOnlyList<LedgerRecord> ledgerItems = blockChainModel
            .GetBlockType<LedgerRecord>();

        return new InstallmentContract
        {
            Creator = header.Creator,
            Description = header.Description,

            NumPayments = details.NumPayments,
            Principal = details.Principal,
            Payment = details.Payment,
            StartDate = details.StartDate,
            CompletedDate = details.CompletedDate,

            Parties = parties,
            Ledger = ledgerItems,
        };
    }
}
