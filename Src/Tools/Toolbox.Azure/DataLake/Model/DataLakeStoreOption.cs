﻿using Toolbox.Tools;

namespace Toolbox.Azure.DataLake.Model;

public record DatalakeStoreOption
{
    public string AccountName { get; init; } = null!;

    public string AccountKey { get; init; } = null!;

    public string ContainerName { get; init; } = null!;

    public string? BasePath { get; init; }
}


public static class DatalakeStoreOptionExtensions
{
    public static DatalakeStoreOption Verify(this DatalakeStoreOption option)
    {
        option.VerifyNotNull(nameof(option));

        option.AccountName.VerifyNotEmpty(nameof(option.AccountName));
        option.AccountKey.VerifyNotEmpty(nameof(option.AccountKey));
        option.ContainerName.VerifyNotEmpty(nameof(option.ContainerName));

        return option;
    }
}