using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Storage;

[GenerateSerializer, Immutable]
public sealed record StorageBlobInfo
{
    [Id(0)] public string StorageId { get; init; } = null!;
    [Id(1)] public string BlobHash { get; init; } = null!;

    public static IValidator<StorageBlobInfo> Validator { get; } = new Validator<StorageBlobInfo>()
        .RuleFor(x => x.StorageId).ValidResourceId(ResourceType.DomainOwned)
        .RuleFor(x => x.BlobHash).NotEmpty()
        .Build();
}

public static class StorageBlobInfoExtensions
{
    public static Option Validate(this StorageBlobInfo subject) => StorageBlobInfo.Validator.Validate(subject).ToOptionStatus();
}
