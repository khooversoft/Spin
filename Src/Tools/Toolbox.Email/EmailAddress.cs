using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Email;

public readonly struct EmailAddress
{
    public EmailAddress(string name, string email) => (Name, Email) = (name.NotEmpty(), email.NotEmpty());

    public string Name { get; }
    public string Email { get; }

    public static implicit operator EmailAddress((string name, string email) value) => new EmailAddress(value.name, value.email);

    public static IValidator<EmailAddress> Validator { get; } = new Validator<EmailAddress>()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Email).NotEmpty()
        .Build();
}


public static class EmailAddressExtensions
{
    public static Option Validate(this EmailAddress subject) => EmailAddress.Validator.Validate(subject).ToOptionStatus();
}