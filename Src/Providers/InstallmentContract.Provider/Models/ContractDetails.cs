using Toolbox.Model;
using Toolbox.Protocol;
using Toolbox.Tools;
using Toolbox.Types;

namespace InstallmentContract.Provider.Models;

public sealed record ContractDetails
{
    public DocumentId DocumentId { get; init; } = null!;
    public Guid ContractId { get; init; } = Guid.NewGuid();
    public string PrincipleId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Issuer { get; init; } = null!;
    public string Description { get; init; } = null!;
    public InitialDetail Initial { get; init; } = null!;
    public IReadOnlyList<PartyRecord> Parties { get; init; } = Array.Empty<PartyRecord>();
    public IReadOnlyList<LedgerRecord> Ledgers { get; init; } = Array.Empty<LedgerRecord>();
    public IReadOnlyList<ConfigEntry> Configurations { get; init; } = Array.Empty<ConfigEntry>();

    public bool Equals(ContractDetails? obj)
    {
        return obj is ContractDetails contract &&
               EqualityComparer<DocumentId>.Default.Equals(DocumentId, contract.DocumentId) &&
               ContractId == contract.ContractId &&
               PrincipleId == contract.PrincipleId &&
               Name == contract.Name &&
               Issuer == contract.Issuer &&
               Description == contract.Description &&
               Initial == contract.Initial &&
               Enumerable.SequenceEqual(Parties, contract.Parties) &&
               Enumerable.SequenceEqual(Ledgers, contract.Ledgers);
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, Initial, Parties, Ledgers);
}


public static class InstallmentContractExtensions
{
    public static bool IsValid(this ContractDetails subject) =>
        subject != null &&
        subject.Initial.IsValid() &&
        subject.Parties != null &&
        subject.Ledgers != null;

    public static decimal GetBalance(this ContractDetails ledgerRecords)
    {
        ledgerRecords.NotNull();

        return ledgerRecords.Ledgers
            .Select(x => x.NaturalAmount())
            .Sum(x => x);
    }
}