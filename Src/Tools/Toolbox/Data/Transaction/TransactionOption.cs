using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class TransactionOption
{
    public string ListSpaceName { get; init; } = null!;
    public string JournalKey { get; init; } = null!;

    public static IValidator<TransactionOption> Validator { get; } = new Validator<TransactionOption>()
        .RuleFor(x => x.ListSpaceName).NotEmpty()
        .RuleFor(x => x.JournalKey).NotEmpty()
        .Build();
}

public static class TransactionOptionExtensions
{
    public static Option Validate(this TransactionOption option) => TransactionOption.Validator.Validate(option).ToOptionStatus();
}

