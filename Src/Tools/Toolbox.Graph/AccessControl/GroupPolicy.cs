//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public record GroupPolicy
//{
//    public GroupPolicy(string nameIdentifier, string principalIdentifier)
//    {
//        NameIdentifier = nameIdentifier.NotEmpty();
//        PrincipalIdentifier = principalIdentifier.NotEmpty();

//        Key = CreateKey(nameIdentifier, principalIdentifier);
//    }

//    public string Key { get; }
//    public string NameIdentifier { get; }
//    public string PrincipalIdentifier { get; }

//    public static IValidator<GroupPolicy> Validator { get; } = new Validator<GroupPolicy>()
//        .RuleFor(x => x.NameIdentifier).NotEmpty()
//        .RuleFor(x => x.PrincipalIdentifier).NotEmpty()
//        .Build();

//    public static string CreateKey(string nameIdentifier, string principalIdentifier) => $"policy:{nameIdentifier}-{principalIdentifier}";
//}

//public static class SecurityGroupTool
//{
//    public static Option Validate(this GroupPolicy subject) => GroupPolicy.Validator.Validate(subject).ToOptionStatus();
//}
