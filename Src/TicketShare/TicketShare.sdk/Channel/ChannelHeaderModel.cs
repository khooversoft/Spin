using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record ChannelHeaderModel
{
    public string ChannelId { get; set; } = null!;
    public string Name { get; set; } = null!;

    public static IValidator<ChannelHeaderModel> Validator { get; } = new Validator<ChannelHeaderModel>()
        .RuleFor(x => x.Name).Must(ChannelHeaderTool.ValidateName)
        .Build();
}


public static class ChannelHeaderTool
{
    public static Option Validate(this ChannelHeaderModel subject) => ChannelHeaderModel.Validator.Validate(subject).ToOptionStatus();

    public static Option ValidateName(string value) => StandardValidation.IsDescrption(value) switch
    {
        true => StatusCode.OK,
        false => (StatusCode.BadRequest, StandardValidation.NameError),
    };

    public static ChannelHeaderModel Clone(this ChannelHeaderModel subject) => new ChannelHeaderModel
    {
        ChannelId = subject.ChannelId,
        Name = subject.Name,
    };
}