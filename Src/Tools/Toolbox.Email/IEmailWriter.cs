using Toolbox.Types;

namespace Toolbox.Email;

public interface IEmailWriter
{
    Task<Option> WriteText(IEnumerable<EmailAddress> toEmails, string subject, string textMessage, ScopeContext context);
    Task<Option> WriteHtml(IEnumerable<EmailAddress> toEmails, string subject, string htmlText, ScopeContext context);
}
