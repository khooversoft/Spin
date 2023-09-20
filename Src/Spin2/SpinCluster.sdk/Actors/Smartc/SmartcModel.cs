using SpinCluster.sdk.Models;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

// smartc:{name}
[GenerateSerializer, Immutable]
public sealed record SmartcModel
{
    [Id(0)] public string SmartcId { get; init; } = null!;
    [Id(1)] public DateTime Registered { get; init; } = DateTime.UtcNow;
    [Id(2)] public string SmartcExeId { get; init; } = null!;
    [Id(3)] public string ContractId { get; init; } = null!;
    [Id(4)] public bool Enabled { get; init; }
    [Id(5)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(6)] public string BlobHash { get; init; } = null!;
    [Id(7)] public IReadOnlyList<PackageFile> PackageFiles { get; init; } = Array.Empty<PackageFile>();

    public bool IsActive => Enabled;

    public bool Equals(SmartcModel? obj) => obj is SmartcModel document &&
        SmartcId == document.SmartcId &&
        Registered == document.Registered &&
        SmartcExeId == document.SmartcExeId &&
        SmartcId == document.SmartcId &&
        Enabled == document.Enabled &&
        CreatedDate == document.CreatedDate &&
        BlobHash == document.BlobHash &&
        PackageFiles.SequenceEqual(document.PackageFiles);

    public override int GetHashCode() => HashCode.Combine(SmartcId, Registered, SmartcExeId);


    public static IValidator<SmartcModel> Validator { get; } = new Validator<SmartcModel>()
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .RuleFor(x => x.Registered).ValidDateTime()
        .RuleFor(x => x.SmartcExeId).ValidResourceId(ResourceType.DomainOwned, "smartc-exe")
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, "contract")
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.BlobHash).NotEmpty()
        .RuleFor(x => x.PackageFiles).Must(x => x.Count > 0, _ => "PackageFiles is empty")
        .RuleForEach(x => x.PackageFiles).Validate(PackageFile.Validator)
        .Build();
}

public sealed record PackageFile
{
    [Id(1)] public string File { get; init; } = null!;
    [Id(2)] public string FileHash { get; init; } = null!;

    public static IValidator<PackageFile> Validator { get; } = new Validator<PackageFile>()
        .RuleFor(x => x.File).NotEmpty()
        .RuleFor(x => x.FileHash).NotEmpty()
        .Build();
}

public static class SmartcModelExtensions
{
    public static Option Validate(this SmartcModel model) => SmartcModel.Validator.Validate(model).ToOptionStatus();
    public static Option Validate(this PackageFile model) => PackageFile.Validator.Validate(model).ToOptionStatus();
}

