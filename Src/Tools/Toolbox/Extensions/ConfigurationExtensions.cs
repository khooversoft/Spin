using Microsoft.Extensions.Configuration;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class ConfigurationExtensions
{
    public static T Get<T>(this IConfiguration configuration, string sectionName) where T : new()
    {
        configuration.NotNull();
        sectionName.NotEmpty();

        var result = configuration.GetSection(sectionName).Get<T>();
        result.NotNull($"Section {sectionName} not found");

        return result;
    }

    public static T Get<T>(this IConfiguration configuration, string sectionName, IValidator<T> validator) where T : new()
    {
        validator.NotNull();

        var result = configuration.Get<T>(sectionName);
        validator.Validate(result).ThrowOnError($"sectionName={sectionName}, type={typeof(T).Name}");

        return result;
    }
}
