using System.Text.Json;

namespace Domain.Errors;

public abstract class HandleError
{
    public string Type { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; protected set; }

    protected HandleError(string type)
    {
        Type = type;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}