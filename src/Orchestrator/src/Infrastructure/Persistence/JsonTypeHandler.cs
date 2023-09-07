using System.Data;
using System.Text.Json;

using static Dapper.SqlMapper;

namespace Infrastructure.Persistence;

public class DictionaryStringToStringTypeHandler : TypeHandler<Dictionary<string, string>>
{
    public override void SetValue(IDbDataParameter parameter, Dictionary<string, string> value) =>
        throw new NotImplementedException();

    public override Dictionary<string, string> Parse(object value) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>((string)value)!;
}

public class DictionaryStringToObjectTypeHandler : TypeHandler<Dictionary<string, object>>
{
    public override void SetValue(IDbDataParameter parameter, Dictionary<string, object> value) =>
        throw new NotImplementedException();

    public override Dictionary<string, object> Parse(object value) =>
        JsonSerializer.Deserialize<Dictionary<string, object>>((string)value)!;
}
