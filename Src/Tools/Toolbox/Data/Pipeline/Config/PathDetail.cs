using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public record PathDetail
{
    public string PipelineName { get; init; } = null!;
    public string? TypeName { get; init; }
    public string Key { get; init; } = null!;

    public override string ToString() => $"{PipelineName}/{TypeName}/{Key}".ToLower();

    public bool Like(PathDetail subject) => PipelineName.Like(subject.PipelineName) &&
        (TypeName == null || TypeName.Like(subject.TypeName)) &&
        Key.Like(subject.PipelineName);

    public static IValidator<PathDetail> Validator { get; } = new Validator<PathDetail>()
        .RuleFor(x => x.PipelineName).NotEmpty()
        .RuleFor(x => x.Key).NotEmpty()
        .Build();
}

public static class PathDetailTool
{
    public static Option Validate(this PathDetail subject) => PathDetail.Validator.Validate(subject).ToOptionStatus();

    public static string GetKey(this PathDetail subject) => subject switch
    {
        not null => $"{subject.NotNull().PipelineName}/{subject.TypeName}/{subject.Key}",
        null => $"{subject.NotNull().PipelineName}/{subject.Key}",
    };
}

