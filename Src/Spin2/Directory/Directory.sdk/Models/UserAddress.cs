using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Directory.sdk.Models;

public record UserAddress
{
    public string Type { get; init; } = null!;
    public string Address1 { get; init; } = null!;
    public string? Address2 { get; init; }
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string ZipCode { get; init; } = null!;
    public string Country { get; init; } = null!;
}


public static class UserAddressValidator
{
    public static Validator<UserAddress> Validator { get; } = new Validator<UserAddress>()
        .RuleFor(x => x.Type).NotEmpty()
        .RuleFor(x => x.Address1).NotEmpty()
        .RuleFor(x => x.City).NotEmpty()
        .RuleFor(x => x.State).NotEmpty()
        .RuleFor(x => x.ZipCode).NotEmpty()
        .RuleFor(x => x.Country).NotEmpty()
        .Build();

    public static bool IsValid(this UserAddress subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .IsValid(location);
}