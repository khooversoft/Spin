using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Store.List;

public record ListStoreOption
{
    public FileSystemType SystemType { get; init; }

    public static IValidator<ListStoreOption> Validator { get; } = new Validator<ListStoreOption>()
        .RuleFor(x => x.SystemType).ValidEnum()
        .Build();
}
