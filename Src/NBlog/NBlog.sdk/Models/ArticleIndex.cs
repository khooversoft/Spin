//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace NBlog.sdk;

//public record ArticleIndex
//{
//    public IReadOnlyList<ArticleReference> Articles { get; init; } = Array.Empty<ArticleReference>();

//    public static IValidator<ArticleIndex> Validator { get; } = new Validator<ArticleIndex>()
//        .RuleForEach(x => x.Articles).Validate(ArticleReference.Validator)
//        .Build();
//}


//public record ArticleReference
//{
//    public string ArticleId { get; init; } = null!;
//    public string FileId { get; init; } = null!;
//    public DateTime CreatedDate { get; init; }
//    public IReadOnlyList<string> Attributes { get; init; } = Array.Empty<string>();
//    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

//    public static IValidator<ArticleReference> Validator { get; } = new Validator<ArticleReference>()
//        .RuleFor(x => x.ArticleId).Must(x => NBlog.sdk.FileId.Create(x).IsOk(), x => $"Invalid FileId {x}")
//        .RuleFor(x => x.FileId).Must(x => NBlog.sdk.FileId.Create(x).IsOk(), x => $"Invalid FileId {x}")
//        .RuleFor(x => x.Attributes).NotNull()
//        .RuleFor(x => x.Tags).NotNull()
//        .Build();
//}

//public record AttributeReference
//{
//    public string Attribute { get; init; } = null!;
//    public IReadOnlyList<string> ArticleId
//}

//public record ArticleFile
//{
//    public string ArticleId { get; init; } = null!;
//    public string FileId { get; init; } = null!;
//}


//public static class ArticleIndexExtensions
//{
//    public static Option Validate(this ArticleIndex subject) => ArticleIndex.Validator.Validate(subject).ToOptionStatus();

//    public static bool Validate(this ArticleIndex subject, out Option result)
//    {
//        result = subject.Validate();
//        return result.IsOk();
//    }
//}