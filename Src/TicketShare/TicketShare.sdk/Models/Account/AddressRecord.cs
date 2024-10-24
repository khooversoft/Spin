using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record AddressRecord
{
    public string Label { get; init; } = null!;
    public string? Address1 { get; init; } = null!;
    public string? Address2 { get; init; }
    public string? City { get; init; } = null!;
    public string? State { get; init; } = null!;
    public string? ZipCode { get; init; } = null!;

    public static IValidator<AddressRecord> Validator { get; } = new Validator<AddressRecord>()
        .RuleFor(x => x.Label).NotEmpty()
        .Build();
}

public static class AddressRecordExtensions
{
    public static Option Validate(this AddressRecord subject) => AddressRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this AddressRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static bool HasData(this AddressRecord subject) =>
        subject.Address1.IsNotEmpty() ||
        subject.Address2.IsNotEmpty() ||
        subject.City.IsNotEmpty() ||
        subject.State.IsNotEmpty() ||
        subject.ZipCode.IsNotEmpty();
}