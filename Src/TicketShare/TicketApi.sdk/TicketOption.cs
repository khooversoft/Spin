using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public record TicketOption
{
    public string ApiKey { get; init; } = null!;
    public int BatchSize { get; init; } = 100;
    public string EventUrl { get; init; } = null!;
    public string ClassificationUrl { get; init; } = null!;
    public string AttractionUrl { get; init; } = null!;
    public bool OnlyHomeGames { get; init; }
    public IReadOnlyList<ImageSelect> ImageSelectors { get; init; } = Array.Empty<ImageSelect>();
    public IReadOnlyList<string> Include { get; init; } = Array.Empty<string>();
    public IReadOnlyList<LogoOption> Logos { get; init; } = Array.Empty<LogoOption>();
    public IReadOnlyList<string> IncludeLogoFiles { get; init; } = Array.Empty<string>();


    public static IValidator<TicketOption> Validator => new Validator<TicketOption>()
        .RuleFor(x => x.ApiKey).NotEmpty()
        .RuleFor(x => x.EventUrl).NotEmpty()
        .RuleFor(x => x.BatchSize).Must(x => x > 10, x => $"BatchSize {x} must be greater than 10")
        .RuleFor(x => x.ClassificationUrl).NotEmpty()
        .RuleFor(x => x.AttractionUrl).NotEmpty()
        .RuleFor(x => x.ImageSelectors).NotNull()
        .RuleForEach(x => x.ImageSelectors).Validate(ImageSelect.Validator)
        .RuleFor(x => x.Include).NotNull()
        .RuleForEach(x => x.Logos).Validate(LogoOption.Validator)
        .RuleFor(x => x.IncludeLogoFiles).NotNull()
        .Build();
}

public record ImageSelect
{
    public string Ratio { get; init; } = null!;
    public int Width { get; init; }
    public int Height { get; init; }

    public static IValidator<ImageSelect> Validator { get; } = new Validator<ImageSelect>()
        .RuleFor(x => x.Ratio).NotEmpty()
        .RuleFor(x => x.Width).Must(x => x > 0, _ => "Width must be greater than 0")
        .RuleFor(x => x.Height).Must(x => x > 0, _ => "Height must be greater than 0")
        .Build();
}

public record LogoOption
{
    public string MatchTo { get; init; } = null!;
    public string LogoUrl { get; init; } = null!;

    public static IValidator<LogoOption> Validator { get; } = new Validator<LogoOption>()
        .RuleFor(x => x.MatchTo).NotEmpty()
        .RuleFor(x => x.LogoUrl).NotEmpty()
        .Build();
}

public record LogoCollection
{
    public IReadOnlyList<LogoOption> Logos { get; init; } = Array.Empty<LogoOption>();
}

public static class TicketMasterOptionExtensions
{
    public static Option Validate(this TicketOption subject) => TicketOption.Validator.Validate(subject).ToOptionStatus();

    public static bool ShouldInclude(this TicketOption subject, string path)
    {
        if (subject.Include.Count == 0) return true;

        foreach (var include in subject.Include)
        {
            if (include.EqualsIgnoreCase(path)) return true;
            if (path.Like(include)) return true;
        }

        return false;
    }

    //public static bool IsImageSelected(this TicketOption subject, ImageModel imageModel) => subject.ImageSelectors.Any(x => x.IsImageSelected(imageModel));

    //private static bool IsImageSelected(this ImageSelect imageSelect, ImageModel imageModel) =>
    //    imageSelect.Ratio.EqualsIgnoreCase(imageModel.Ratio) &&
    //    imageModel.Width <= imageSelect.Width &&
    //    imageModel.Height <= imageSelect.Height;

    //public static bool IsImageSelected(this TicketOption subject, ImageRecord imageModel) => subject.ImageSelectors.Any(x => x.IsImageSelected(imageModel));

    //private static bool IsImageSelected(this ImageSelect imageSelect, ImageRecord imageRecord) =>
    //    imageSelect.Ratio.EqualsIgnoreCase(imageRecord.Ratio) &&
    //    imageRecord.Width <= imageSelect.Width &&
    //    imageRecord.Height <= imageSelect.Height;
}

