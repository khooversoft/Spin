using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum PropsoalStatus
{
    None,
    Accepted,
    Rejected
}

public record ProposalResponse
{
    public PropsoalStatus Status { get; init; }
    public string ByPrincipalId { get; init; } = null!;
    public DateTime AcceptedDate { get; init; }

    public static IValidator<ProposalResponse> Validator { get; } = new Validator<ProposalResponse>()
        .RuleFor(x => x.Status).ValidEnum()
        .RuleFor(x => x.ByPrincipalId).NotEmpty()
        .RuleFor(x => x.AcceptedDate).ValidDateTime()
        .Build();
}

public static class ProposalResponseTool
{
    public static Option Validate(this ProposalResponse subject) => ProposalResponse.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ProposalResponse subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}