//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public record TransactionManagerOption
//{
//    public string JournalKey { get; init; } = null!;

//    public static IValidator<TransactionManagerOption> Validator { get; } = new Validator<TransactionManagerOption>()
//        .RuleFor(x => x.JournalKey).NotEmpty()
//        .Build();
//}

//public static class TransactionManagerOptionExtensions
//{
//    public static Option Validate(this TransactionManagerOption option) => TransactionManagerOption.Validator.Validate(option).ToOptionStatus();
//}

