using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

// smartc:{name}
[GenerateSerializer, Immutable]
public record SmartcModel
{
    [Id(0)] public string SmartcId { get; init; } = null!;
    [Id(1)] public DateTime Registered { get; init; } = DateTime.UtcNow;
    [Id(2)] public bool Enabled { get; init; }

    public static IValidator<SmartcModel> Validator { get; } = new Validator<SmartcModel>()
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .Build();
}


public static class SmartcModelExtensions
{
    public static Option Validate(this SmartcModel model) => SmartcModel.Validator.Validate(model).ToOptionStatus();
}