using SpinCluster.sdk.Models;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

// smartc:{domain}/{path}[/{path}...]
[GenerateSerializer, Immutable]
public sealed record SmartcModel
{
    [Id(0)] public string SmartcId { get; init; } = null!;
    [Id(1)] public DateTime Registered { get; init; } = DateTime.UtcNow;
    [Id(2)] public string SmartcExeId { get; init; } = null!;
    [Id(3)] public string ContractId { get; init; } = null!;
    [Id(4)] public string Executable { get; init; } = null!;
    [Id(5)] public bool Enabled { get; init; }
    [Id(6)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(7)] public string BlobHash { get; init; } = null!;
    [Id(8)] public IReadOnlyList<PackageFile> PackageFiles { get; init; } = Array.Empty<PackageFile>();

    public bool IsActive => Enabled;

    public bool Equals(SmartcModel? obj) => obj is SmartcModel document &&
        SmartcId == document.SmartcId &&
        Registered == document.Registered &&
        SmartcExeId == document.SmartcExeId &&
        ContractId == document.ContractId &&
        Executable == document.Executable &&
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
        .RuleFor(x => x.Executable).NotEmpty()
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
    public static Option Validate(this SmartcModel subject) => SmartcModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SmartcModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static Option Validate(this PackageFile subject) => PackageFile.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this PackageFile subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}

