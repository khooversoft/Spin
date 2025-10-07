//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.Extensions;

////
//// PrincipalIdentity =
////    pk = 'user:{PrincipalId}'
////    index = 'email:{Email}'
////    index = 'logonProvider:{LoginProvider}"


//public sealed record PrincipalIdentity
//{
//    // Id - GetById e.g. 'user:9b0d4bed' - IdentityTool.GeneratedNodeKey() - this is the ID used for security
//    public string PrincipalId { get; set; } = null!;
//    public string Email { get; set; } = null!;
//    public string NameIdentifier { get; set; } = null!;
//    public string UserName { get; set; } = null!;
//    public bool EmailConfirmed { get; set; }


//    public static IValidator<PrincipalIdentity> Validator { get; } = new Validator<PrincipalIdentity>()
//        .RuleFor(x => x.PrincipalId).NotEmpty()
//        .RuleFor(x => x.Email).ValidEmail()
//        .RuleFor(x => x.NameIdentifier).NotEmpty()
//        .RuleFor(x => x.UserName).NotEmpty()
//        .Build();
//}


//public static class PrincipalIdentityTool
//{
//    public static Option Validate(this PrincipalIdentity subject) => PrincipalIdentity.Validator.Validate(subject).ToOptionStatus();

//    public static bool HasLoginProvider(this PrincipalIdentity subject) => subject.NotNull().NameIdentifier.IsNotEmpty();
//}
