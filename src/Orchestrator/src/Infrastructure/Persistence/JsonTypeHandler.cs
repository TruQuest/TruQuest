using System.Data;
using System.Text.Json;

using Dapper;

namespace Infrastructure.Persistence;

public class JsonTypeHandler : SqlMapper.TypeHandler<Dictionary<string, string>>
{
    public override void SetValue(IDbDataParameter parameter, Dictionary<string, string> value)
    {
        throw new NotImplementedException();
    }

    public override Dictionary<string, string> Parse(object value)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>((string)value)!;
    }
}