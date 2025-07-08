//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Store;

//public record LocalFileStoreOption
//{
//    public string BasePath { get; init; } = null!;

//    public static IValidator<LocalFileStoreOption> Validator { get; } = new Validator<LocalFileStoreOption>()
//        .RuleFor(x => x.BasePath).NotEmpty()
//        .Build();
//}


//public static class LocalFileStoreOptionExtensions
//{
//    public static Option Validate(this LocalFileStoreOption subject) => LocalFileStoreOption.Validator.Validate(subject).ToOptionStatus();
//}
