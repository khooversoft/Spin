using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Finance.Finance;

public readonly record struct LoanTerms
{
    public decimal PrincipalAmount { get; init; }
    public double APR { get; init; }
    public int NumberPayments { get; init; }
    public int PaymentsPerYear { get; init; }

    public static IValidator<LoanTerms> Validator { get; } = new Validator<LoanTerms>()
        .RuleFor(x => x.PrincipalAmount).Must(x => x > 0.0m, x => $"{x} is invalid")
        .RuleFor(x => x.APR).Must(x => x > 0.0 && x < 1.0, x => $"{x} is invalid")
        .RuleFor(x => x.NumberPayments).Must(x => x > 0, x => $"{x} is invalid")
        .RuleFor(x => x.PaymentsPerYear).Must(x => x > 0, x => $"{x} is invalid")
        .Build();
}

public static class LoanTermsExtensions
{
    public static Option Validate(this LoanTerms subject) => LoanTerms.Validator.Validate(subject).ToOptionStatus();
}