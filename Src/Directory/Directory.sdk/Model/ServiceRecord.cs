﻿using Toolbox.Tools;

namespace Directory.sdk.Model;

public record ServiceRecord
{
    public string HostUrl { get; init; } = null!;

    public string ApiKey { get; init; } = null!;
}


public static class ServiceRecordExtensions
{
    public static ServiceRecord Verify(this ServiceRecord subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.HostUrl.VerifyNotEmpty($"{nameof(subject.HostUrl)} is required");
        subject.ApiKey.VerifyNotEmpty($"{nameof(subject.ApiKey)} is required");

        return subject;
    }
}
