//using Toolbox.Data;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public record GrantGroup
//{
//    public const string NodeType = "group";

//    public GrantGroup(string name)
//    {
//        name.NotEmpty();
//        NodeKey = NodeTool.CreateKey(name, NodeType);
//        Name = name;
//    }

//    public string NodeKey { get; }
//    public string Name { get; }
//    public string? Description { get; }

//    public static IValidator<GrantGroup> Validator { get; } = new Validator<GrantGroup>()
//        .RuleFor(x => x.NodeKey).NotEmpty()
//        .RuleFor(x => x.Name).NotEmpty()
//        .Build();
//}


//public static class GroupDetailTool
//{
//    public static Option Validate(this GrantGroup subject) => GrantGroup.Validator.Validate(subject).ToOptionStatus();
//}
