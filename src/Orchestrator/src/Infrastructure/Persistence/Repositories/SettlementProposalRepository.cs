using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;

namespace Infrastructure.Persistence.Repositories;

internal class SettlementProposalRepository : Repository, ISettlementProposalRepository
{
    private new readonly AppDbContext _dbContext;

    public SettlementProposalRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(SettlementProposal proposal) => _dbContext.SettlementProposals.Add(proposal);

    public Task<SettlementProposal> FindById(Guid id) => _dbContext.SettlementProposals.SingleAsync(p => p.Id == id);

    public Task<SettlementProposalState> GetStateFor(Guid proposalId) =>
        _dbContext.SettlementProposals.Where(p => p.Id == proposalId).Select(p => p.State).SingleAsync();

    public async Task UpdateStateFor(Guid proposalId, SettlementProposalState state)
    {
        var proposalIdParam = new NpgsqlParameter<Guid>("ProposalId", NpgsqlDbType.Uuid)
        {
            TypedValue = proposalId
        };
        var stateParam = new NpgsqlParameter("State", state);

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                UPDATE truquest.""SettlementProposals""
                SET ""State"" = @State
                WHERE ""Id"" = @ProposalId;
            ",
            proposalIdParam, stateParam
        );
    }

    public async Task<IReadOnlyList<SettlementProposalVerifier>> GetAllVerifiersFor(Guid proposalId)
    {
        var settlementProposal = await _dbContext.SettlementProposals
            .AsNoTracking()
            .Include(p => p.Verifiers)
            .SingleAsync(p => p.Id == proposalId);

        return settlementProposal.Verifiers;
    }

    public async Task<bool> CheckIsDesignatedVerifierFor(Guid proposalId, string userId)
    {
        var proposal = await _dbContext.SettlementProposals
            .AsNoTracking()
            .Include(p => p.Verifiers.Where(v => v.VerifierId == userId))
            .Where(p => p.Id == proposalId)
            .SingleAsync();

        return proposal.Verifiers.Any();
    }

    public async Task<Guid> DeepCopyFromWith(Guid sourceProposalId, SettlementProposalState state)
    {
        var sourceProposalIdParam = new NpgsqlParameter<Guid>("SourceProposalId", NpgsqlDbType.Uuid)
        {
            TypedValue = sourceProposalId
        };
        var destProposalIdParam = new NpgsqlParameter<Guid>("DestProposalId", NpgsqlDbType.Uuid)
        {
            TypedValue = Guid.NewGuid()
        };
        var stateParam = new NpgsqlParameter("State", state);

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                INSERT INTO truquest.""SettlementProposals"" (
                    ""Id"", ""ThingId"", ""State"", ""SubmittedAt"",
                    ""Title"", ""Verdict"", ""Details"", ""ImageIpfsCid"",
                    ""CroppedImageIpfsCid"", ""SubmitterId"",
                    ""RelatedProposals""
                )
                    SELECT
                        @DestProposalId, ""ThingId"", @State, ""SubmittedAt"",
                        ""Title"", ""Verdict"", ""Details"", ""ImageIpfsCid"",
                        ""CroppedImageIpfsCid"", ""SubmitterId"",
                        jsonb_build_object('prev', ""Id""::TEXT)
                    FROM truquest.""SettlementProposals""
                    WHERE ""Id"" = @SourceProposalId;

                INSERT INTO truquest.""SettlementProposalEvidence"" (
                    ""Id"", ""SettlementProposalId"", ""OriginUrl"", ""IpfsCid"", ""PreviewImageIpfsCid""
                )
                    SELECT gen_random_uuid(), @DestProposalId, ""OriginUrl"", ""IpfsCid"", ""PreviewImageIpfsCid""
                    FROM truquest.""SettlementProposalEvidence""
                    WHERE ""SettlementProposalId"" = @SourceProposalId;
            ",
            sourceProposalIdParam, destProposalIdParam, stateParam
        );

        return destProposalIdParam.TypedValue;
    }
}
