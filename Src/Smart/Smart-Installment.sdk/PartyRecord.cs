using Toolbox.Block.Serialization;
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

    public static DataItem ConvertTo(this PartyRecord subject)
    {
        subject.NotNull();

        return new DataItem
        {
            Id = subject.Id,
            Key = subject.GetType().Name,
            Value = subject.ToJson(),
        };
    }

    public static PartyRecord ConvertTo(this DataItem dataItem) => dataItem
        .NotNull()
        .Assert(x => x.Key == typeof(PartyRecord).Name, "Invalid type")
        .Value
        .ToObject<PartyRecord>(true)!;
}

