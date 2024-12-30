using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum ContactType
{
    Phone,
    Email,
}

public record ContactRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public ContactType Type { get; init; } = ContactType.Phone;
    public string Value { get; init; } = null!;

    public static IValidator<ContactRecord> Validator { get; } = new Validator<ContactRecord>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Type).ValidEnum()
        .RuleFor(x => x.Value).NotEmpty()
        .Build();
}

public static class ContactRecordExtensions
{
    public static Option Validate(this ContactRecord subject) => ContactRecord.Validator.Validate(subject).ToOptionStatus();
}