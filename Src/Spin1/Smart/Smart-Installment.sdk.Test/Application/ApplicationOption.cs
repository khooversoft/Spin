namespace Smart_Installment.sdk.Test.Application;

public record ApplicationOption
{
    public RunEnvironment RunEnvironment { get; init; }

    public string DirectoryUrl { get; init; } = null!;

    public string DirectoryApiKey { get; init; } = null!;

    public string ContractUrl { get; init; } = null!;

    public string ContractApiKey { get; init; } = null!;
}

