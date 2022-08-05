using Contract.sdk.Client;
using Contract.sdk.Models;
using Contract.sdk.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Actor;
using Toolbox.DocumentStore;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Smart_Installment.sdk.Actor;

public class ContractStoreActor : ActorBase, IContractStoreActor
{
    private readonly ContractClient _contractClient;

    public ContractStoreActor(ContractClient contractClient)
    {
        _contractClient = contractClient.NotNull();
    }

    public async Task Create(InstallmentHeader header, CancellationToken token)
    {
        header.Verify();

        var contractCreate = new ContractCreateModel
        {
            PrincipleId = header.PrincipleId,
            DocumentId = (string)ActorKey,
            Creator = header.Issuer,
            Description = header.Description,
            Name = header.Name,
        };

        await _contractClient.Create(contractCreate, token);
        await _contractClient.Append(ActorKey.ToDocumentId(), header, header.PrincipleId, token);
    }

    public async Task<InstallmentContract?> Get(CancellationToken token)
    {
        DocumentId documentId = ActorKey.ToDocumentId();

        var blockTypes = new BlockTypeRequest()
            .Add<InstallmentHeader>()
            .Add<DataGroup<PartyRecord>>(true)
            .Add<DataGroup<LedgerRecord>>(true);

        IReadOnlyList<DataBlockResult>? documents = await _contractClient.Get(documentId, blockTypes, token);
        if (documents == null) return null;

        InstallmentHeader? header = documents.GetLast<InstallmentHeader>();
        if (header == null) return null;

        IReadOnlyList<DataGroup<PartyRecord>>? partyRecords = documents.GetAll<DataGroup<PartyRecord>>();
        if (partyRecords == null) return null;

        IReadOnlyList<DataGroup<LedgerRecord>>? ledgerRecords = documents.GetAll<DataGroup<LedgerRecord>>();
        if (ledgerRecords == null) return null;

        return new InstallmentContract
        {
            DocumentId = documentId,
            Header = header,
            PartyRecords = new TrxList<PartyRecord>
            {
                Committed = partyRecords.SelectMany(x => x.Items).ToList(),
            },
            LedgerRecords = new TrxList<LedgerRecord>
            {
                Committed = ledgerRecords.SelectMany(x => x.Items).ToList(),
            }
        };
    }

    public async Task Append(InstallmentContract contract, CancellationToken token)
    {
        contract.Verify();
        DocumentId documentId = ActorKey.ToDocumentId();

        InstallmentHeader? header = await _contractClient.GetLatest<InstallmentHeader>(documentId, token);
        header.Assert(x => x != null, $"Installment contract {contract.DocumentId} not found");

        var batch = new DocumentBatch()
            .SetDocumentId(contract.DocumentId);


        // Update header if different
        if (contract.Header != header) batch.Add(contract.Header, contract.Header.PrincipleId);


        if (contract.PartyRecords.Items.Count > 0)
        {
            var pGroup = new DataGroup<PartyRecord>
            {
                Items = contract.PartyRecords.Items.ToList(),
            };

            batch.Add(pGroup, contract.Header.PrincipleId);
        }

        if (contract.LedgerRecords.Items.Count > 0)
        {
            var lGroup = new DataGroup<LedgerRecord>
            {
                Items = contract.LedgerRecords.Items.ToList(),
            };

            batch.Add(lGroup, contract.Header.PrincipleId);
        }

        if (batch.Count == 0) return;

        AppendResult appendResult = await _contractClient.Append(batch, token);
        appendResult.ErrorCount.Assert(x => x == 0, "Append failed");
    }
}
