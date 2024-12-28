using System.Collections.Immutable;
using TicketShare.sdk;
using Fluent = Microsoft.FluentUI.AspNetCore.Components;

namespace TicketShareWeb.Components.Pages.Profile.Models;

public sealed record ContactModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
}


public static class ContactModelTool
{
    public static ContactModel Clone(this ContactModel subject) => new ContactModel
    {
        Id = subject.Id,
        Type = subject.Type,
        Value = subject.Value,
    };

    public static ContactModel ConvertTo(this ContactRecord subject) => new ContactModel
    {
        Id = subject.Id,
        Type = subject.Type.ToString(),
        Value = subject.Value,
    };

    public static ContactRecord ConvertTo(this ContactModel subject) => new ContactRecord
    {
        Id = subject.Id,
        Type = Enum.Parse<ContactType>(subject.Type),
        Value = subject.Value,
    };

    public static IReadOnlyList<Fluent.Option<string>> ValidContactTypes = Enum.GetNames<ContactType>()
        .OrderBy(x => x)
        .Select(x => new Fluent.Option<string> { Value = x, Text = x })
        .ToImmutableArray();
}