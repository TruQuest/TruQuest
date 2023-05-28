using Microsoft.Extensions.Configuration;

using Dapper;

using Domain.Aggregates;

namespace Tests.FunctionalTests.Helpers;

public class DbQueryable : Infrastructure.Persistence.Queryables.Queryable
{
    public DbQueryable(IConfiguration configuration) : base(configuration) { }

    public async Task<byte[]> GetOrchestratorCommitmentForThing(Guid thingId)
    {
        var dbConn = await _getOpenConnection();
        var data = await dbConn.QuerySingleAsync<string>(
            @"
                SELECT ""Payload""->>'data'
                FROM truquest.""Tasks""
                WHERE ""Type"" = @Type AND ""Payload""->>'thingId' = @ThingId
            ",
            param: new
            {
                Type = (int)TaskType.CloseThingSubmissionVerifierLottery,
                ThingId = thingId.ToString()
            }
        );

        return Convert.FromBase64String(data);
    }
}