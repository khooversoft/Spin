using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
public record AddressRecord
{
    [Id(0)] public string AddressRecordId { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public string Address1 { get; init; } = null!;
    [Id(2)] public string? Address2 { get; init; }
    [Id(3)] public string City { get; init; } = null!;
    [Id(4)] public string State { get; init; } = null!;
    [Id(5)] public string ZipCode { get; init; } = null!;

    public static IValidator<AddressRecord> Validator { get; } = new Validator<AddressRecord>()
        .RuleFor(x => x.AddressRecordId).NotEmpty()
        .RuleFor(x => x.Address1).NotEmpty()
        .RuleFor(x => x.City).NotEmpty()
        .RuleFor(x => x.State).NotEmpty()
        .RuleFor(x => x.ZipCode).NotEmpty()
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
}