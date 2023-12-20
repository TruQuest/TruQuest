using System.Reflection;

namespace Common.Monitoring;

public static class ActivityNames
{
    private static IReadOnlyDictionary<Type, string> _typeToActivityName;

    public static void Populate()
    {
        var typeToActivityName = new Dictionary<Type, string>();
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            var ns = type.Namespace ?? string.Empty;
            var shortNs = string.Join('.', ns.Split('.').Select(n => n.Substring(0, Math.Min(3, n.Length))));
            typeToActivityName[type] = shortNs + (shortNs.Length > 0 ? $".{type.Name}" : type.Name);
        }
        _typeToActivityName = typeToActivityName;
    }

    public static string GetActivityName(this Type type) => _typeToActivityName[type];
}
