using Directory.sdk;
using Directory.sdk.Client;
using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using Toolbox.Abstractions;
using Toolbox.Abstractions.Extensions;
using Toolbox.Abstractions.Tools;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools.Property;

namespace DirectoryCmd.Activities;

internal class SetActivity
{
    private readonly DirectoryClient _directoryClient;
    private readonly IdentityClient _identityClient;
    private readonly ILogger<SetActivity> _logger;

    public SetActivity(DirectoryClient directoryClient, IdentityClient identityClient, ILogger<SetActivity> logger)
    {
        _directoryClient = directoryClient;
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task SetFile(string file, CancellationToken token)
    {
        file.NotEmpty();

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(SetFile), File = file }).NotNull();

        _logger.LogInformation("Pushing {file} to directory service", file);

        IReadOnlyList<string> includeFiles = ConfigurationTools.GetJsonFiles(file, PropertyResolver.CreateEmpty())
            .Where(x => x != file)
            .ToList();

        IReadOnlyList<DirectoryEntry> list = new DirectoryEntryBuilder()
            .Add(includeFiles)
            .Build();

        foreach (DirectoryEntry entry in list)
        {
            await _directoryClient.Set(entry, token);
            _logger.LogInformation("Writing Id {entry.DirectoryId}", entry.DirectoryId);

            if (entry.ClassType == ClassTypeName.User)
            {
                await CreateIdentity(entry, token);
            }
        }

        _logger.LogInformation("Completed writing {list.Count} entries from {file} to directory", list.Count, file);
    }

    public async Task SetProperty(string directoryId, string[] properties, CancellationToken token)
    {
        var id = new DocumentId(directoryId);
        _logger.LogInformation("Updating property {properties} on {directoryId}", properties.Join(", "), directoryId);

        using IDisposable scope = _logger.BeginScope(new { Command = nameof(SetProperty), DirectoryId = directoryId, Properties = properties }).NotNull();

        DirectoryEntry entry = (await _directoryClient.Get(id)) ?? new DirectoryEntry { DirectoryId = (string)id };

        var propertyList = new List<string>(entry.Properties);

        foreach (var item in properties)
        {
            string property = item
                .ToKeyValuePair()
                .ToKeyValueString();

            propertyList.Add(property);
        }

        await _directoryClient.Set(entry, token);
        _logger.LogInformation("Updated property {properties} on {directoryId}", properties.Join(", "), directoryId);
    }

    private async Task CreateIdentity(DirectoryEntry entry, CancellationToken token)
    {
        string? email = entry.GetEmail();
        string? identityId = entry.GetSigningCredentials();

        if (email == null && identityId == null) return;

        if (!(email != null && identityId != null))
        {
            _logger.LogError("Directory Id {entry.DirectoryId} must specify both {PropertyName.Email} and {PropertyName.SigningCredentials} properties",
                entry.DirectoryId,
                PropertyName.Email,
                PropertyName.SigningCredentials
                );

            return;
        }

        var identityEntryRequest = new IdentityEntryRequest
        {
            DirectoryId = identityId,
            Issuer = email,
        };

        await _identityClient.Create(identityEntryRequest, token);
    }
}
