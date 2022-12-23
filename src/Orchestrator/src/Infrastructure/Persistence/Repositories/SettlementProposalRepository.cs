using Microsoft.Extensions.Configuration;

using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class SettlementProposalRepository : Repository<SettlementProposal>, ISettlementProposalRepository
{
    private readonly AppDbContext _dbContext;

    public SettlementProposalRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(SettlementProposal proposal)
    {
        _dbContext.SettlementProposals.Add(proposal);
    }

    public Task<SettlementProposal> FindById(Guid id) => _dbContext.SettlementProposals.SingleAsync(p => p.Id == id);

    public async Task<IReadOnlyList<SettlementProposalVerifier>> GetAllVerifiersFor(Guid settlementProposalId)
    {
        var settlementProposal = await _dbContext.SettlementProposals
            .AsNoTracking()
            .Include(p => p.Verifiers)
            .SingleAsync(p => p.Id == settlementProposalId);

        return settlementProposal.Verifiers;
    }
}