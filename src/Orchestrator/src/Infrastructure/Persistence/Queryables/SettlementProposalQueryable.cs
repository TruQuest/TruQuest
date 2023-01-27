using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates;
using Application.Settlement.Queries.GetSettlementProposals;
using Application.Common.Interfaces;
using Application.Settlement.Queries.GetSettlementProposal;
using Application.Common.Models.QM;

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

    public Task<SettlementProposalQm?> GetById(Guid id)
    {
        return _dbContext.SettlementProposals
            .Where(p => p.Id == id)
            .Select(p => new SettlementProposalQm
            {
                Id = p.Id,
                ThingId = p.ThingId,
                State = p.State,
                Title = p.Title,
                Verdict = p.Verdict,
                Details = p.Details,
                ImageIpfsCid = p.ImageIpfsCid,
                CroppedImageIpfsCid = p.CroppedImageIpfsCid,
                SubmitterId = p.SubmitterId,
                Evidence = p.Evidence.Select(e => new SupportingEvidenceQm
                {
                    OriginUrl = e.OriginUrl,
                    IpfsCid = e.IpfsCid,
                    PreviewImageIpfsCid = e.PreviewImageIpfsCid
                }).ToList(),
            })
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<VerifierLotteryParticipantEntryQm>> GetVerifierLotteryParticipants(
        Guid proposalId
    )
    {
        var dbConn = await _getOpenConnection();
        var entries = await dbConn.QueryAsync<VerifierLotteryParticipantEntryQm>(
            @"
                SELECT je.""BlockNumber"" AS ""JoinedBlockNumber"", pje.""UserId"", pje.""DataHash"", je.""Nonce""
                FROM
                    truquest_events.""PreJoinedThingAssessmentVerifierLotteryEvents"" AS pje
                        LEFT JOIN
                    truquest_events.""JoinedThingAssessmentVerifierLotteryEvents"" AS je
                        ON (
                            pje.""ThingId"" = je.""ThingId"" AND
                            pje.""SettlementProposalId"" = je.""SettlementProposalId"" AND
                            pje.""UserId"" = je.""UserId""
                        )
                WHERE pje.""SettlementProposalId"" = @SettlementProposalId
                ORDER BY je.""BlockNumber"" DESC NULLS LAST, je.""TxnIndex"" DESC
            ",
            param: new
            {
                SettlementProposalId = proposalId
            }
        );

        return entries;
    }
}