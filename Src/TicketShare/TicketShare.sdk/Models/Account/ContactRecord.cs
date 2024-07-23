using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum ContactType
{
    Cell,
    Email,
    Text
}

[GenerateSerializer]
[Alias("TicketShare.sdk.ContactRecord")]
public record ContactRecord
{
    [Id(0)] public ContactType Type { get; init; } = ContactType.Cell;
    [Id(1)] public string Value { get; init; } = null!;

    public static IValidator<ContactRecord> Validator { get; } = new Validator<ContactRecord>()
        .RuleFor(x => x.Type).ValidEnum()
        .RuleFor(x => x.Value).NotEmpty()
        .Build();
}

public static class ContactRecordExtensions
{
    public static Option Validate(this ContactRecord subject) => ContactRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ContactRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}