using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Toolbox.Types;

namespace Toolbox.Tools;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class JsonRegisterAttribute : Attribute
{
    public JsonRegisterAttribute(Type registeredType) => RegisteredType = registeredType;
    public Type RegisteredType { get; }
}

public static class JsonSerializerContextRegistered
{
    private static ConcurrentDictionary<string, JsonTypeInfo> _store = new(StringComparer.OrdinalIgnoreCase);
    private static int _scanned;
    private static int _assemblyLoadSubscribed;

    public static void ScanAndRegister()
    {
        while (true)
        {
            int state = Volatile.Read(ref _scanned);
            if (state == 2) return;

            if (Interlocked.CompareExchange(ref _scanned, 1, 0) != 0)
            {
                var spinner = new SpinWait();
                while (Volatile.Read(ref _scanned) == 1) spinner.SpinOnce();
                continue;
            }

            try
            {
                var scannedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var store = ScanAllAssemblies(scannedAssemblies);
                Interlocked.Exchange(ref _store, store);
                Volatile.Write(ref _scanned, 2);

                EnsureAssemblyLoadSubscribed();

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName is null) continue;
                    if (scannedAssemblies.Contains(assembly.FullName)) continue;

                    ScanAssembly(assembly, _store, strict: false);
                }

                return;
            }
            catch
            {
                Volatile.Write(ref _scanned, 0);
                throw;
            }
        }
    }

    [DebuggerStepThrough]
    public static Option<JsonTypeInfo> Find<T>()
    {
        ScanAndRegister();

        string key = typeof(T).Name;
        return _store.TryGetValue(key, out var jsonTypeInfo) ? jsonTypeInfo : StatusCode.NotFound;
    }

    private static void EnsureAssemblyLoadSubscribed()
    {
        if (Interlocked.CompareExchange(ref _assemblyLoadSubscribed, 1, 0) != 0) return;

        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
    }

    private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs e)
    {
        if (Volatile.Read(ref _scanned) != 2) return;

        try
        {
            ScanAssembly(e.LoadedAssembly, _store, strict: false);
        }
        catch
        {
            // AssemblyLoad event handlers should not throw.
        }
    }

    private static ConcurrentDictionary<string, JsonTypeInfo> ScanAllAssemblies(HashSet<string>? scannedAssemblies = null)
    {
        var store = new ConcurrentDictionary<string, JsonTypeInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName is not null) scannedAssemblies?.Add(assembly.FullName);
            ScanAssembly(assembly, store, strict: true);
        }

        return store;
    }

    private static void ScanAssembly(Assembly assembly, ConcurrentDictionary<string, JsonTypeInfo> store, bool strict)
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
            if (!typeof(JsonSerializerContext).IsAssignableFrom(type)) continue;

            JsonRegisterAttribute[] registerAttributes = type.GetCustomAttributes<JsonRegisterAttribute>(inherit: false).ToArray();
            if (registerAttributes.Length == 0) continue;

            // Get the generated context instance via its static Default property
            object? contextInstance = type.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (contextInstance is null) continue;

            foreach (JsonRegisterAttribute registerAttribute in registerAttributes)
            {
                Type registeredType = registerAttribute.RegisteredType;
                string memberName = registeredType.Name;

                // Prefer property (STJ source generator emits properties for type infos)
                if (type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance) is PropertyInfo pi)
                {
                    if (pi.GetValue(contextInstance) is JsonTypeInfo jsonTypeInfo)
                    {
                        if (strict) store.TryAdd(memberName, jsonTypeInfo).BeTrue();
                        else store.TryAdd(memberName, jsonTypeInfo);
                    }

                    continue;
                }

                // Allow method pattern for completeness (no parameters)
                if (type.GetMethod(memberName, BindingFlags.Public | BindingFlags.Instance) is MethodInfo mi && mi.GetParameters().Length == 0)
                {
                    if (mi.Invoke(contextInstance, null) is JsonTypeInfo jsonTypeInfo)
                    {
                        if (strict) store.TryAdd(memberName, jsonTypeInfo).BeTrue();
                        else store.TryAdd(memberName, jsonTypeInfo);
                    }

                    continue;
                }

                if (strict) throw new ArgumentException($"Registered type {memberName} is not a property or parameterless method");
            }
        }
    }
}
