using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;
public record TicketGroupHeaderModel
{
    public string? TicketGroupId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public static IValidator<TicketGroupHeaderModel> Validator { get; } = new Validator<TicketGroupHeaderModel>()
        .RuleFor(x => x.Name).Must(TicketGroupRecordTool.ValidateName)
        .RuleFor(x => x.Description).Must(TicketGroupRecordTool.ValidateDescription)
        .Build();
}


public static class TicketGroupDetailModelExtensions
{
    public static Option Validate(this TicketGroupHeaderModel subject) => TicketGroupHeaderModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this TicketGroupHeaderModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static TicketGroupHeaderModel Clone(this TicketGroupHeaderModel subject) => new TicketGroupHeaderModel
    {
        Name = subject.Name,
        Description = subject.Description,
    };

    public static TicketGroupHeaderModel ConvertToModel(this TicketGroupModel subject) => new TicketGroupHeaderModel
    {
        Name = subject.Name,
        Description = subject.Description,
    };

    public static TicketGroupModel ConvertTo(this TicketGroupHeaderModel subject) => new TicketGroupModel
    {
        Name = subject.Name,
        Description = subject.Description,
    };
}