using TicketApi.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public record TicketOption
{
    public string ApiKey { get; init; } = null!;
    public string EventUrl { get; init; } = null!;
    public string ClassificationUrl { get; init; } = null!;
    public string AttriactionUrl { get; init; } = null!;
    public IReadOnlyList<ImageSelect> ImageSelectors { get; init; } = Array.Empty<ImageSelect>();

    public static IValidator<TicketOption> Validator => new Validator<TicketOption>()
        .RuleFor(x => x.ApiKey).NotEmpty()
        .RuleFor(x => x.EventUrl).NotEmpty()
        .RuleFor(x => x.ClassificationUrl).NotEmpty()
        .RuleFor(x => x.AttriactionUrl).NotEmpty()
        .RuleFor(x => x.ImageSelectors).NotNull()
        .RuleForEach(x => x.ImageSelectors).Validate(ImageSelect.Validator)
        .Build();
}

public static class TicketMasterOptionExtensions
{
    public static Option Validate(this TicketOption subject) => TicketOption.Validator.Validate(subject).ToOptionStatus();

    public static bool IsImageSelected(this TicketOption subject, ImageModel imageModel) => subject.ImageSelectors.Any(x => x.IsImageSelected(imageModel));
    private static bool IsImageSelected(this ImageSelect imageSelect, ImageModel imageModel) =>
        imageSelect.Ratio.EqualsIgnoreCase(imageModel.Ratio) &&
        imageModel.Width <= imageSelect.Width &&
        imageModel.Height <= imageSelect.Height;

    public static bool IsImageSelected(this TicketOption subject, ImageRecord imageModel) => subject.ImageSelectors.Any(x => x.IsImageSelected(imageModel));
    private static bool IsImageSelected(this ImageSelect imageSelect, ImageRecord imageRecord) =>
        imageSelect.Ratio.EqualsIgnoreCase(imageRecord.Ratio) &&
        imageRecord.Width <= imageSelect.Width &&
        imageRecord.Height <= imageSelect.Height;
}


public record ImageSelect
{
    public string Ratio { get; init; } = null!;
    public int Width { get; init; }
    public int Height { get; init; }

    public static IValidator<ImageSelect> Validator => new Validator<ImageSelect>()
        .RuleFor(x => x.Ratio).NotEmpty()
        .RuleFor(x => x.Width).Must(x => x > 0, _ => "Width must be greater than 0")
        .RuleFor(x => x.Height).Must(x => x > 0, _ => "Height must be greater than 0")
        .Build();
}