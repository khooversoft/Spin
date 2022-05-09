﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Azure.Table;

public class TableOption
{
    public string AccountName { get; set; } = null!;

    public string AccountKey { get; set; } = null!;
}


public static class TableOptionExtensions
{
    public static void Verify(this TableOption subject)
    {
        subject.NotNull(nameof(subject));

        subject.AccountName.NotEmpty(nameof(subject.AccountName));
        subject.AccountKey.NotEmpty(nameof(subject.AccountKey));
    }
}
