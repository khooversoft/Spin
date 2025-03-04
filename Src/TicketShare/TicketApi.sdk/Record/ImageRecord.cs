namespace TicketApi.sdk;
using TicketApi.sdk.Model;
using Toolbox.Tools;
using Toolbox.Types;

public record ImageRecord
{
    public string Url { get; init; } = null!;
    public string Ratio { get; init; } = null!;
    public int Width { get; init; }
    public int Height { get; init; }
    public string ReferenceId { get; init; } = null!;

    public static IValidator<ImageRecord> Validator { get; } = new Validator<ImageRecord>()
        .RuleFor(x => x.Ratio).NotEmpty()
        .RuleFor(x => x.Url).NotEmpty()
        .RuleFor(x => x.Width).Must(x => x > 0, _ => "Width must be greater than 0")
        .RuleFor(x => x.Height).Must(x => x > 0, _ => "Width must be greater than 0")
        .RuleFor(x => x.ReferenceId).NotEmpty()
        .Build();
}

public static class ImageRecordExtensions
{
    public static Option Validate(this ImageRecord subject) => ImageRecord.Validator.Validate(subject).ToOptionStatus();

    public static ImageRecord ConvertTo(this ImageModel subject, string referenceId) => new ImageRecord
    {
        Ratio = subject.Ratio,
        Url = subject.Url,
        Width = subject.Width,
        Height = subject.Height,
        ReferenceId = referenceId.NotEmpty(),
    };
}
