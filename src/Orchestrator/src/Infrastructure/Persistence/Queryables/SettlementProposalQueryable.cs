using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Settlement.Queries.GetSettlementProposals;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Queryables;

internal class SettlementProposalQueryable : Queryable, ISettlementProposalQueryable
{
    private readonly AppDbContext _dbContext;

    public SettlementProposalQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<SettlementProposalPreviewQm>> GetAllExceptDraftsOfOthersFor(Guid thingId, string userId) =>
        _dbContext.SettlementProposals
            .AsNoTracking()
            .Where(p => p.ThingId == thingId && (p.State > SettlementProposalState.Draft || p.SubmitterId == userId))
            .Select(p => new SettlementProposalPreviewQm
            {
                Id = p.Id,
                State = p.State,
                Title = p.Title,
                Verdict = p.Verdict,
                CroppedImageIpfsCid = p.CroppedImageIpfsCid,
                SubmitterId = p.SubmitterId
            })
            .ToListAsync();

    public Task<List<SettlementProposalPreviewQm>> GetAllExceptDraftsFor(Guid thingId) =>
        _dbContext.SettlementProposals
            .AsNoTracking()
            .Where(p => p.ThingId == thingId && p.State > SettlementProposalState.Draft)
            .Select(p => new SettlementProposalPreviewQm
            {
                Id = p.Id,
                State = p.State,
                Title = p.Title,
                Verdict = p.Verdict,
                CroppedImageIpfsCid = p.CroppedImageIpfsCid,
                SubmitterId = p.SubmitterId
            })
            .ToListAsync();
}