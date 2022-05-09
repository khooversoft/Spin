using Toolbox.Tools;

namespace Smart_Installment.sdk;

public record PartyRecord
{
    public string TrxCode { get; init; } = "add";
    public string UserId { get; init; } = null!;
    public string PartyType { get; init; } = null!;
    public string BankAccountId { get; init; } = null!;
}


public static class PartyExtensions
{
    public static PartyRecord Verify(this PartyRecord subject)
    {
        subject.NotNull(nameof(subject));

        subject.TrxCode.NotEmpty(nameof(subject.TrxCode));
        subject.UserId.NotEmpty(nameof(subject.UserId));
        subject.PartyType.NotEmpty(nameof(subject.PartyType));
        subject.BankAccountId.NotEmpty(nameof(subject.BankAccountId));

        return subject;
    }
}

