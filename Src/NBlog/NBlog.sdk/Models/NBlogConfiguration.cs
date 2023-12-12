using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public class NBlogConfiguration
{
    [Id(0)] public string RootPath { get; init; } = null!;

    public static IValidator<NBlogConfiguration> Validator { get; } = new Validator<NBlogConfiguration>()
        .RuleFor(x => x.RootPath).Must(x => FileId.Create(x).IsOk(), _ => "Invalid root path")
        .Build();
}


public static class NBlogConfigurationExtentions
{
    public static Option Validate(this NBlogConfiguration subject) => NBlogConfiguration.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this NBlogConfiguration subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}