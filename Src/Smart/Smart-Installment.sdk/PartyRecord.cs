using Toolbox.Extensions;
using Toolbox.Tools;

namespace Smart_Installment.sdk;

public record PartyRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string TrxCode { get; init; } = "add";
    public string UserId { get; init; } = null!;
    public string PartyType { get; init; } = null!;
    public string BankAccountId { get; init; } = null!;
}


public static class PartyExtensions
{
    public static PartyRecord Verify(this PartyRecord subject)
    {
        subject.NotNull();

        subject.TrxCode.NotEmpty();
        subject.UserId.NotEmpty();
        subject.PartyType.NotEmpty();
        subject.BankAccountId.NotEmpty();

        return subject;
    }
}

