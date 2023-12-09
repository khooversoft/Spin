//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Tools.Validation;
//using Toolbox.Types;

//namespace Toolbox.Orleans.Types;

//public static class SpinResponseExtensions
//{
//    public static SpinResponse ToSpinResponse(this ValidatorResult value) => new SpinResponse(value.IsValid ? StatusCode.OK : StatusCode.BadRequest, value.FormatErrors());
//    public static SpinResponse ToSpinResponse(this IOption option) => new SpinResponse(option.StatusCode, option.Error);
//    public static SpinResponse<T> ToSpinResponse<T>(this IOption option) => new SpinResponse<T>(option.StatusCode, option.Error);

//    public static T Return<T>(this SpinResponse<T> subject) => subject.StatusCode.IsOk() switch
//    {
//        true => subject.Value.NotNull(),
//        false => throw new ArgumentException("Value is null"),
//    };

//    public static SpinResponse ToSpinResponse(this Option subject) => new SpinResponse(subject.StatusCode, subject.Error);
//}