using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types.ID;

namespace Toolbox.Types;


// user1@company3.com
// user:user1@company3.com
// kid:user1@company3.com/path
// principal-key:user1@company3.com"
// principal-private-key:user1@company3.com"
// tenant:company3.com
// subscription:company3.com/subscriptionId
public readonly record struct ResourceId
{
    [JsonConstructor]
    public ResourceId(string id)
        : this(ResourceIdTool.Parse(id).ThrowOnError().Return())
    {
    }

    private ResourceId(ResourceIdTool.ResourceIdParsed result)
    {
        Id = result.Id;
        Schema = result.Schema;
        User = result.User;
        Domain = result.Domain;
        Path = result.Path;
    }

    public string Id { get; }
    [JsonIgnore] public string? Schema { get; }
    [JsonIgnore] public string? User { get; }
    [JsonIgnore] public string? Domain { get; }
    [JsonIgnore] public string? Path { get; }

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

    public static bool IsValid(string id) => ResourceIdTool.Parse(id).IsOk();

    public static Option<ResourceId> Create(string id)
    {
        var result = ResourceIdTool.Parse(id);
        if (result.IsError()) return result.ToOptionStatus<ResourceId>();

        var resourceId = new ResourceId(result.Return());

        var validation = ResourceIdValidator.Validator.Validate(resourceId);
        if (!validation.IsValid) return new Option<ResourceId>(StatusCode.BadRequest, validation.FormatErrors());

        return resourceId;
    }
}

public static class ResourceIdValidator
{
    public static IValidator<ResourceId> Validator { get; } = new Validator<ResourceId>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Schema).Must(x => x.IsEmpty() || IdPatterns.IsSchema(x), x => $"{x} not valid schema")
        .RuleFor(x => x.User).Must(x => x.IsEmpty() || IdPatterns.IsName(x), x => $"{x} not valid user")
        .RuleFor(x => x.Domain).Must(x => x.IsEmpty() || IdPatterns.IsDomain(x), x => $"{x} not valid domain")
        .RuleFor(x => x.Path).Must(x => x.IsEmpty() || x.Split('/').All(y => IdPatterns.IsPath(y)), x => $"{x} not valid path")
        .Build();

    public static Option<ResourceId> IsValid(this ResourceId subject) => Validator.Validate(subject) switch
    {
        { IsValid: true } => subject,
        var v => new Option<ResourceId>(StatusCode.BadRequest, v.FormatErrors()),
    };
}