using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class ThingRepository : Repository, IThingRepository
{
    private new readonly AppDbContext _dbContext;

    public ThingRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(Thing thing)
    {
        _dbContext.Things.Add(thing);
    }

    public Task<Thing> FindById(Guid id) => _dbContext.Things.SingleAsync(t => t.Id == id);

    public async Task<bool> CheckIsDesignatedVerifierFor(Guid thingId, string userId)
    {
        var thing = await _dbContext.Things
            .AsNoTracking()
            .Include(t => t.Verifiers.Where(v => v.VerifierId == userId))
            .Where(t => t.Id == thingId)
            .SingleAsync();

        return thing.Verifiers.Any();
    }

    public async Task<IReadOnlyList<ThingVerifier>> GetAllVerifiersFor(Guid thingId)
    {
        var thing = await _dbContext.Things
            .AsNoTracking()
            .Include(t => t.Verifiers)
            .SingleAsync(t => t.Id == thingId);

        return thing.Verifiers;
    }

    public async Task<Guid> DeepCopyFromWith(Guid sourceThingId, ThingState state)
    {
        var sourceThingIdParam = new NpgsqlParameter<Guid>("SourceThingId", NpgsqlDbType.Uuid)
        {
            TypedValue = sourceThingId
        };
        var destThingIdParam = new NpgsqlParameter<Guid>("DestThingId", NpgsqlDbType.Uuid)
        {
            TypedValue = Guid.NewGuid()
        };
        var stateParam = new NpgsqlParameter<int>("State", NpgsqlDbType.Integer)
        {
            TypedValue = (int)state
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                INSERT INTO truquest.""Things"" (
                    ""Id"", ""State"", ""SubmittedAt"",
                    ""Title"", ""Details"", ""ImageIpfsCid"",
                    ""CroppedImageIpfsCid"", ""SubmitterId"", ""SubjectId"",
                    ""RelatedThings""
                )
                    SELECT
                        @DestThingId, @State, ""SubmittedAt"",
                        ""Title"", ""Details"", ""ImageIpfsCid"",
                        ""CroppedImageIpfsCid"", ""SubmitterId"", ""SubjectId"",
                        jsonb_build_object('prev', ""Id""::TEXT)
                    FROM truquest.""Things""
                    WHERE ""Id"" = @SourceThingId;

                INSERT INTO truquest.""Evidence"" (
                    ""Id"", ""ThingId"", ""OriginUrl"", ""IpfsCid"", ""PreviewImageIpfsCid""
                )
                    SELECT gen_random_uuid(), @DestThingId, ""OriginUrl"", ""IpfsCid"", ""PreviewImageIpfsCid""
                    FROM truquest.""Evidence""
                    WHERE ""ThingId"" = @SourceThingId;

                INSERT INTO truquest.""ThingAttachedTags"" (""ThingId"", ""TagId"")
                    SELECT @DestThingId, ""TagId""
                    FROM truquest.""ThingAttachedTags""
                    WHERE ""ThingId"" = @SourceThingId
            ",
            sourceThingIdParam, destThingIdParam, stateParam
        );

        return destThingIdParam.TypedValue;
    }
}
