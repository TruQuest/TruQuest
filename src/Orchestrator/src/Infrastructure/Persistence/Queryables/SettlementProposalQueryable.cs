using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates;
using Application.Thing.Queries.GetSettlementProposalsList;
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

    public async Task<List<SettlementProposalPreviewQm>> GetForThing(Guid thingId, string? userId)
    {
        var proposals = await _dbContext.SettlementProposals
            .AsNoTracking()
            .Where(p => p.ThingId == thingId && (p.State > SettlementProposalState.Draft || p.SubmitterId == userId))
            .Select(p => new SettlementProposalPreviewQm
            {
                Id = p.Id,
                State = p.State,
                SubmittedAt = p.SubmittedAt,
                Title = p.Title,
                Verdict = p.Verdict,
                CroppedImageIpfsCid = p.CroppedImageIpfsCid,
                SubmitterId = p.SubmitterId,
                AssessmentPronouncedAt = p.AssessmentPronouncedAt
            })
            .ToListAsync();

        proposals.ForEach(p => p.DisplayedTimestamp = p.AssessmentPronouncedAt ?? p.SubmittedAt);

        return proposals;
    }

    public Task<SettlementProposalQm?> GetById(Guid id)
    {
        return _dbContext.SettlementProposals
            .Where(p => p.Id == id)
            .Select(p => new SettlementProposalQm
            {
                Id = p.Id,
                ThingId = p.ThingId,
                State = p.State,
                SubmittedAt = p.SubmittedAt,
                Title = p.Title,
                Verdict = p.Verdict,
                Details = p.Details,
                ImageIpfsCid = p.ImageIpfsCid,
                CroppedImageIpfsCid = p.CroppedImageIpfsCid,
                SubmitterId = p.SubmitterId,
                AssessmentPronouncedAt = p.AssessmentPronouncedAt,
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
                SELECT je.""BlockNumber"" AS ""JoinedBlockNumber"", ""UserId"", pje.""DataHash"", je.""Nonce""
                FROM
                    truquest_events.""PreJoinedThingAssessmentVerifierLotteryEvents"" AS pje
                        LEFT JOIN
                    truquest_events.""JoinedThingAssessmentVerifierLotteryEvents"" AS je
                        USING (""ThingId"", ""SettlementProposalId"", ""UserId"")
                WHERE ""SettlementProposalId"" = @SettlementProposalId
                ORDER BY je.""BlockNumber"" DESC NULLS LAST, je.""TxnIndex"" DESC
            ",
            param: new
            {
                SettlementProposalId = proposalId
            }
        );

        return entries;
    }

    public async Task<IEnumerable<VerifierQm>> GetVerifiers(Guid proposalId)
    {
        var dbConn = await _getOpenConnection();
        var verifiers = await dbConn.QueryAsync<VerifierQm>(
            @"
                SELECT v.""VerifierId"", u.""UserName""
                FROM
                    truquest.""SettlementProposals"" AS p
                        INNER JOIN
                    truquest.""SettlementProposalVerifiers"" AS v
                        ON p.""Id"" = v.""SettlementProposalId""
                        INNER JOIN
                    truquest.""AspNetUsers"" AS u
                        ON v.""VerifierId"" = u.""Id""
                WHERE p.""Id"" = @ProposalId
            ",
            param: new { ProposalId = proposalId }
        );

        return verifiers;
    }
}