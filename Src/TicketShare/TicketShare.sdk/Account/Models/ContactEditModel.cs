using Microsoft.AspNetCore.Components;
using Toolbox.Tools;

namespace TicketShare.sdk;

public sealed record ContactEditModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
    public EventCallback OnDelete { get; set; }
}

public static class ContactEditModelExtensions
{
    public static ContactEditModel ConvertToEdit(this ContactModel subject) => new ContactEditModel
    {
        Id = subject.NotNull().Id,
        Type = subject.Type.ToString(),
        Value = subject.Value,
    };

    public static ContactModel ConvertTo(this ContactEditModel subject) => new ContactModel
    {
        Id = subject.NotNull().Id,
        Type = subject.Type,
        Value = subject.Value,
    };
}