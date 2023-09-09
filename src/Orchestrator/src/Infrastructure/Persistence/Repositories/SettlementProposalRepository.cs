using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class SettlementProposalRepository : Repository, ISettlementProposalRepository
{
    private new readonly AppDbContext _dbContext;

    public SettlementProposalRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(dbContext, sharedTxnScope)
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
        var stateParam = new NpgsqlParameter<int>("State", NpgsqlDbType.Integer)
        {
            TypedValue = (int)state
        };

        await _dbContext.Database.ExecuteSqlRawAsync(
            @"
                UPDATE truquest.""SettlementProposals""
                SET ""State"" = @State
                WHERE ""Id"" = @ProposalId;
            ",
            proposalIdParam, stateParam
        );
    }

    public async Task<IReadOnlyList<SettlementProposalVerifier>> GetAllVerifiersFor(Guid settlementProposalId)
    {
        var settlementProposal = await _dbContext.SettlementProposals
            .AsNoTracking()
            .Include(p => p.Verifiers)
            .SingleAsync(p => p.Id == settlementProposalId);

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
}
