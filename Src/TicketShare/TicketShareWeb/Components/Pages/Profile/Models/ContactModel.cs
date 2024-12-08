using System.Diagnostics.CodeAnalysis;
using TicketShare.sdk;

namespace TicketShareWeb.Components.Pages.Profile.Models;

public sealed record ContactModel : IEqualityComparer<ContactModel>
{
    public string Id { get; init; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;

    public bool Equals(ContactModel? x, ContactModel? y)
    {
        throw new NotImplementedException();
    }

    public int GetHashCode([DisallowNull] ContactModel obj)
    {
        throw new NotImplementedException();
    }
}


public static class ContactModelExtensions
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
}