//using Toolbox.Extensions;
//using Toolbox.Tools.Validation.Validators;
//using Toolbox.Types.Maybe;

//namespace Toolbox.Tools.Validation;

//public class ForEach<T, TProperty> : IPropertyRule<T, TProperty>, IValidator<T>
//{
//    private readonly IPropertyRule<T, TProperty> _rule;

//    public ForEach(IPropertyRule<T, TProperty> rule)
//    {
//        _rule = rule.NotNull();
//    }

//    public IList<IValidator<T>> Validators { get; init; } = new List<IValidator<T>>();

//    public string Name => throw new NotImplementedException();

//    public Func<T, TProperty> GetValue => throw new NotImplementedException();

//    public Option<IValidateResult> Validate(T subject)
//    {
//        IEnumerable<TProperty>? result = _rule.GetValue(subject) as IEnumerable<TProperty>;
//        if (result == null) throw new ArgumentException("Cannot convert to enumerable");

//        foreach (var item in result.WithIndex())
//        {
//            var itemResult = Validators
//                .Select(x => x.Validate(subject))
//                .Where(x => x.HasValue)
//                .Select(x => x.Return())
//                .FirstOrDefaultOption();

//            if( itemResult.HasValue) return itemResult;
//        }

//        return Option<IValidateResult>.None;
//    }
//}


//public static class ForEachExtensions
//{
//    public static Rule<T, TProperty> ForEach<T, TProperty>(this Rule<T, TProperty> rule)
//    {
//        var validator = new ForEach<T, TProperty>(rule.PropertyRule);
//        rule.PropertyRule.Validators.Add(validator);

//        return new Rule<T, TProperty>
//        {
//            Validator = rule.Validator,
//            PropertyRule = validator,
//        };
//    }

//}