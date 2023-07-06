using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Types;

public static class SpinResponseExtensions
{
    public static SpinResponse ToSpinResponse(this ValidatorResult value) => new SpinResponse(value.IsValid ? StatusCode.OK : StatusCode.BadRequest, value.FormatErrors());
    public static SpinResponse ToSpinResponse(this IOption option) => new SpinResponse(option.StatusCode, option.Error);
    public static SpinResponse<T> ToSpinResponse<T>(this IOption option) => new SpinResponse<T>(option.StatusCode, option.Error);

    public static T Return<T>(this SpinResponse<T> subject) => subject.StatusCode.IsOk() switch
    {
        true => subject.Value.NotNull(),
        false => throw new ArgumentException("Value is null"),
    };

    public static Option<T> ToOption<T>(this SpinResponse<T> subject) => new Option<T>(subject.Value, subject.StatusCode, subject.Error);
    public static Option<T> ToOption<T>(this ISpinResponseWithValue subject) => new Option<T>((T)subject.ValueObject, subject.StatusCode, subject.Error);

    public static async Task<SpinResponse> UnwrapAsync(this Task<Option<SpinResponse>> subject)
    {
        Option<SpinResponse> result = await subject;

        return result.IsError() switch
        {
            true => new SpinResponse(result.StatusCode, result.Error),
            false => result.Return(),
        };
    }

    public static async Task<SpinResponse<T>> UnwrapAsync<T>(this Task<Option<SpinResponse<T>>> subject)
    {
        Option<SpinResponse<T>> result = await subject;

        return result.IsError() switch
        {
            true => new SpinResponse<T>(result.StatusCode, result.Error),
            false => result.Return(),
        };
    }
}