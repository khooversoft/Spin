using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Tools;

public record StaticImage
{
    public string Name { get; init; } = null!;
    public byte[] Data { get; init; } = null!;

    public string GetBase64() => Convert.ToBase64String(Data);

    public string GetImageSource() => $"data:image/webp;base64,{GetBase64()}";

    public static IValidator<StaticImage> Validator { get; } = new Validator<StaticImage>()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.Data).Must(x => x != null && x.Length > 0, _ => "Data is null or empty")
        .Build();
}


public static class StaticImageTool
{
    public static Option Validate(this StaticImage subject) => StaticImage.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this StaticImage subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}