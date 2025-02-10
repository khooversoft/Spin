using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Email;

public interface IEmailSender
{
    Task<Option> SendText(EmailAddress toEmail, string subject, string textMessage, ScopeContext context);
    Task<Option> SendHtml(EmailAddress toEmail, string subject, string htmlText, ScopeContext context);
}

public readonly struct EmailAddress
{
    public EmailAddress(string name, string email) => (Name, Email) = (name.NotEmpty(), email.NotEmpty());

    public string Name { get; }
    public string Email { get; }

    public static implicit operator EmailAddress((string name, string email) value) => new EmailAddress(value.name, value.email);
}
