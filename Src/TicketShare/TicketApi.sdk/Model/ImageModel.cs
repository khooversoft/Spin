using Toolbox.Tools;

namespace TicketApi.sdk.Model;

public record ImageModel
{
    public string Ratio { get; init; } = null!;
    public string Url { get; init; } = null!;
    public int Width { get; init; }
    public int Height { get; init; }

    public static IValidator<ImageModel> Validator { get; } = new Validator<ImageModel>()
        .RuleFor(x => x.Ratio).NotEmpty()
        .RuleFor(x => x.Url).NotEmpty()
        .RuleFor(x => x.Width).Must(x => x > 0, _ => "Width must be greater than 0")
        .RuleFor(x => x.Height).Must(x => x > 0, _ => "Width must be greater than 0")
        .Build();
}
