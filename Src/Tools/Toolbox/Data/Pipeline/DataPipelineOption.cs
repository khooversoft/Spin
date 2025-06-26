//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public record DataPipelineOption
//{
//    public TimeSpan MemoryCacheDuration { get; set; } = TimeSpan.FromMinutes(60);
//    public TimeSpan? FileCacheDuration { get; set; }
//    public string BasePath { get; set; } = null!;

//    public static IValidator<DataPipelineOption> Validator { get; } = new Validator<DataPipelineOption>()
//        .RuleFor(o => o.MemoryCacheDuration).Must(x => x > TimeSpan.FromMinutes(1), _ => "MemoryCacheDuration must be greater than 1 minute")
//        .RuleFor(o => o.BasePath).NotEmpty()
//        .Build();
//}

//public static class DataPipelineOptionExtensions
//{
//    public static Option Validate(this DataPipelineOption subject) => DataPipelineOption.Validator.Validate(subject).ToOptionStatus();
//}
