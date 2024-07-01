using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
public record AddressRecord
{
    [Id(0)] public string AddressRecordId { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public string? Address1 { get; init; } = null!;
    [Id(2)] public string? Address2 { get; init; }
    [Id(3)] public string? City { get; init; } = null!;
    [Id(4)] public string? State { get; init; } = null!;
    [Id(5)] public string? ZipCode { get; init; } = null!;

    public static IValidator<AddressRecord> Validator { get; } = new Validator<AddressRecord>()
        .RuleFor(x => x.AddressRecordId).NotEmpty()
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

    public static bool IsMatch(this AddressRecord subject, AddressRecord addressRecord)
    {
        bool match = compare(subject.Address1, addressRecord.Address1) &&
            compare(subject.Address2, addressRecord.Address2) &&
            compare(subject.City, addressRecord.City) &&
            compare(subject.State, addressRecord.State) &&
            compare(subject.ZipCode, addressRecord.ZipCode);

        return match;

        static bool compare(string? left, string? right) => (left.ToNullIfEmpty(), right.ToNullIfEmpty()) switch
        {
            (null, null) => true,
            (string v1, string v2) => v1.EqualsIgnoreCase(v2),
            _ => false,
        };
    }
}