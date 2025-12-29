using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class TransactionOption
{
    public string ListSpaceName { get; set; } = null!;
    public string JournalKey { get; set; } = null!;
    public List<Func<IServiceProvider, ITrxProvider>> Providers { get; } = new();

    public static IValidator<TransactionOption> Validator { get; } = new Validator<TransactionOption>()
        .RuleFor(x => x.ListSpaceName).NotEmpty()
        .RuleFor(x => x.JournalKey).NotEmpty()
        .RuleFor(x => x.Providers).NotNull()
        .Build();
}

public static class TransactionOptionExtensions
{
    public static Option Validate(this TransactionOption option) => TransactionOption.Validator.Validate(option).ToOptionStatus();

    public static void Add<T>(this List<Func<IServiceProvider, ITrxProvider>> providers) where T : ITrxProvider
    {
        providers.Add(sp => sp.GetRequiredService<T>());
    }
}

