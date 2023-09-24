using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Finance.Finance;

public readonly record struct InterestChargeDetail
{
    public decimal Principal { get; init; }
    public double APR { get; init; }
    public int NumberOfDays { get; init; }

    public static IValidator<InterestChargeDetail> Validator { get; } = new Validator<InterestChargeDetail>()
        .RuleFor(x => x.Principal).Must(x => x > 0.0m, x => $"{x} is invalid")
        .RuleFor(x => x.APR).Must(x => x > 0.0 && x < 1.0, x => $"{x} is invalid")
        .RuleFor(x => x.NumberOfDays).Must(x => x > 0, x => $"{x} is invalid")
        .Build();
}


public static class InterestChargeDetailExtensions
{
    public static Option Validate(this InterestChargeDetail subject) => InterestChargeDetail.Validator.Validate(subject).ToOptionStatus();
}