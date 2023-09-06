using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types.ID;

namespace Toolbox.Types;

public enum ResourceType
{
    System = 1,     // subscription:{subscriptionName}                                  {schema}:{name}
                    // agent:{agentId}                                                  {schema}:{name}
    Tenant,         // tenant:{domain}                                                  {schema}:{domain}
    Principal,      // {user}@{domain}                                                  {schema}@{domain}
    Owned,          // {schema}:{user}@{domain}/{path}[/{path}...}]                     {schema}:{name}@{domain}[/{path}]
                    //      principal-key:{user}@{domain}/{path}[/{path}...}]
                    //      kid:{user}@{domain}/{path}[/{path}...}]
                    //      signature:{user}@{domain}/{path}[/{path}...}]
    DomainOwned,    // {schema}:{domain}/{path}[/{path}...}]                            {schema}:{domain}/{path}[/{path}]
}


[DebuggerDisplay("Id={Id}")]
public readonly record struct ResourceId
{
    [JsonConstructor]
    public ResourceId(string id)
        : this(ResourceIdTool.Parse(id).ThrowOnError().Return())
    {
    }

    public ResourceId(ResourceId resourceId)
    {
        Id = resourceId.Id;
        Type = resourceId.Type;
        Schema = resourceId.Schema;
        SystemName = resourceId.SystemName;
        User = resourceId.User;
        Domain = resourceId.Domain;
        Path = resourceId.Path;
        PrincipalId = resourceId.PrincipalId;
        AccountId = resourceId.AccountId;
    }

    public string Id { get; init; }
    [JsonIgnore] public ResourceType Type { get; init; }
    [JsonIgnore] public string? Schema { get; init; }
    [JsonIgnore] public string? SystemName { get; init; }
    [JsonIgnore] public string? User { get; init; }
    [JsonIgnore] public string? Domain { get; init; }
    [JsonIgnore] public string? Path { get; init; }
    [JsonIgnore] public string? PrincipalId { get; init; }
    [JsonIgnore] public string? AccountId { get; init; }

    public override string ToString() => Id;
    public string ToUrlEncoding() => Uri.EscapeDataString(Id);
    public bool Equals(ResourceId? obj) => obj is ResourceId subject && Id == subject.Id;
    public override int GetHashCode() => HashCode.Combine(Id);

    public static bool operator ==(ResourceId left, string right) => left.Id.Equals(right);
    public static bool operator !=(ResourceId left, string right) => !(left == right);
    public static bool operator ==(string left, ResourceId right) => left.Equals(right.Id);
    public static bool operator !=(string left, ResourceId right) => !(left == right);

    public static implicit operator ResourceId(string subject) => new ResourceId(subject);
    public static implicit operator string(ResourceId subject) => subject.ToString();

    public static bool IsValid(string id, ResourceType? type = null, string? schema = null) => ResourceIdTool.Parse(id) switch
    {
        { StatusCode: StatusCode.OK } v => v.Return() switch
        {
            var r when type != null && r.Type != type => false,
            var r when schema != null && r.Schema != schema => false,

            _ => true,
        },

        _ => false,
    };


    public static Option<ResourceId> Create(string id) => ResourceIdTool.Parse(id);
}

public static class ResourceIdValidator
{
    public static string GetKeyId(this ResourceId resourceId) => $"{resourceId.User}@{resourceId.Domain}" + resourceId.Path;
}