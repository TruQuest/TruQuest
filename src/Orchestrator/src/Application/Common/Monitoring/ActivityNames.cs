using System.Reflection;

namespace Application.Common.Monitoring;

public static class ActivityNames
{
    private static IReadOnlyDictionary<Type, string> _typeToActivityName;

    public static void Populate(params Assembly[] assemblies)
    {
        var typeToActivityName = new Dictionary<Type, string>();
        foreach (var type in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract)))
        {
            var ns = type.Namespace ?? string.Empty;
            var shortNs = string.Join('.', ns.Split('.').Select(n => n.Substring(0, Math.Min(3, n.Length))));
            typeToActivityName[type] = shortNs + (shortNs.Length > 0 ? $".{type.Name}" : type.Name);
        }
        _typeToActivityName = typeToActivityName;
    }

    public static string GetActivityName(this Type type) => _typeToActivityName[type];
}
