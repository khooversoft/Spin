using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Toolbox.Types;

namespace Toolbox.Tools;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class JsonRegisterAttribute : Attribute
{
    public JsonRegisterAttribute(Type registeredType) => RegisteredType = registeredType;
    public Type RegisteredType { get; }
}

public static class JsonSerializerContextRegistered
{
    private static ConcurrentDictionary<string, JsonTypeInfo> _store = new(StringComparer.OrdinalIgnoreCase);

#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    [ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    public static void ScanAndRegister()
    {
        var store = ScanAllAssemblies();
        Interlocked.Exchange(ref _store, store);
    }

    public static Option<JsonTypeInfo> Find<T>()
    {
        string key = typeof(T).Name;
        return _store.TryGetValue(key, out var jsonTypeInfo) ? jsonTypeInfo : StatusCode.NotFound;
    }

    private static ConcurrentDictionary<string, JsonTypeInfo> ScanAllAssemblies()
    {
        var store = new ConcurrentDictionary<string, JsonTypeInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types = Array.Empty<Type>();

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(x => x != null).Cast<Type>().ToArray();
            }

            foreach (Type type in types)
            {
                JsonRegisterAttribute? registerAttribute = type.GetCustomAttribute<JsonRegisterAttribute>(inherit: false);
                if (registerAttribute is null) continue;
                if (!typeof(JsonSerializerContext).IsAssignableFrom(type)) continue;

                // Get the generated context instance via its static Default property
                object? contextInstance = type.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (contextInstance is null) continue;

                Type registeredType = registerAttribute.RegisteredType;
                string memberName = registeredType.Name;

                // Prefer property (STJ source generator emits properties for type infos)
                MemberInfo? member = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance)
                                   ?? (MemberInfo?)type.GetMethod(memberName, BindingFlags.Public | BindingFlags.Instance);

                if (member is not PropertyInfo pi) throw new ArgumentException($"Registered type {memberName} is not a property");
                if (pi.GetValue(contextInstance) is JsonTypeInfo jsonTypeInfo) store.TryAdd(memberName, jsonTypeInfo).BeTrue();
            }
        }

        return store;
    }
}
