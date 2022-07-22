using Contract.sdk.Client;
using Contract.sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Actor;
using Toolbox.Tools;

namespace Smart_Installment.sdk.Actor;

public class InstallmentContractActor : ActorBase, IInstallmentContractActor
{
    private readonly ContractClient _contractClient;

    public InstallmentContractActor(ContractClient contractClient)
    {
        _contractClient = contractClient.NotNull();
    }

    public async Task CreateContract(InstallmentHeader header, CancellationToken token)
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

        InstallmentHeader? header = await _contractClient.GetLatest<InstallmentHeader>(documentId, token);
        if (header == null) return null;

        IReadOnlyList<DataGroup<PartyRecord>>? partyRecords = await _contractClient.GetAll<DataGroup<PartyRecord>>(documentId, token);
        if (partyRecords == null) return null;

        IReadOnlyList<DataGroup<LedgerRecord>>? ledgerRecords = await _contractClient.GetAll<DataGroup<LedgerRecord>>(documentId, token);
        if (ledgerRecords == null) return null;

        return new InstallmentContract
        {
            DocumentId = documentId,
            Header = header,
            CommittedParties = partyRecords.SelectMany(x => x.Items).ToList(),
            CommittedLedger = ledgerRecords.SelectMany(x => x.Items).ToList(),
        };
    }

    public async Task Append(InstallmentContract contract, CancellationToken token)
    {
        contract.Verify();
        DocumentId documentId = ActorKey.ToDocumentId();

        InstallmentHeader? header = await _contractClient.GetLatest<InstallmentHeader>(documentId, token);
        header.Assert(x => x != null, $"Installment contract {contract.DocumentId} not found");

        if (contract.Header != header) await _contractClient.Append(documentId, contract.Header, contract.Header.PrincipleId, token);

        if (contract.Parties.Count > 0)
        {
            var pGroup = new DataGroup<PartyRecord>
            {
                Items = contract.Parties.ToList(),
            };

            await _contractClient.Append(contract.DocumentId, pGroup, contract.Header.PrincipleId, token);
        }

        if (contract.Ledger.Count > 0)
        {
            var lGroup = new DataGroup<LedgerRecord>
            {
                Items = contract.Ledger.ToList(),
            };

            await _contractClient.Append(contract.DocumentId, lGroup, contract.Header.PrincipleId, token);
        }
    }
}
