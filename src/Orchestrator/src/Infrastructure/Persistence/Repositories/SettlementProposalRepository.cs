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
}