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

    public async Task<SettlementProposalQm?> GetById(Guid id, string? userId)
    {
        var dbConn = await _getOpenConnection();
        using var multiQuery = await dbConn.QueryMultipleAsync(
            @"
                SELECT
                    p.*,
                    s.""Name"" AS ""SubjectName"",
                    t.""Title"" AS ""ThingTitle"", t.""CroppedImageIpfsCid"" AS ""ThingCroppedImageIpfsCid"",
                    e.*
                FROM
                    truquest.""SettlementProposals"" AS p
                        INNER JOIN
                    truquest.""Things"" AS t
                        ON p.""ThingId"" = t.""Id""
                        INNER JOIN
                    truquest.""Subjects"" AS s
                        ON t.""SubjectId"" = s.""Id""
                        INNER JOIN
                    truquest.""SupportingEvidence"" AS e
                        ON p.""Id"" = e.""ProposalId""
                WHERE p.""Id"" = @ItemId;

                SELECT 1
                FROM truquest.""WatchList""
                WHERE
                    (""UserId"", ""ItemType"", ""ItemId"", ""ItemUpdateCategory"") =
                    (@UserId, @ItemType, @ItemId, @ItemUpdateCategory);
            ",
            param: new
            {
                UserId = userId,
                ItemType = (int)WatchedItemType.SettlementProposal,
                ItemId = id,
                ItemUpdateCategory = (int)SettlementProposalUpdateCategory.General
            }
        );

        var proposal = multiQuery.SingleWithMany<SettlementProposalQm, SupportingEvidenceQm>(
            joinedCollectionSelector: proposal => proposal.Evidence
        );
        if (proposal != null)
        {
            proposal.Watched = multiQuery.ReadSingleOrDefault<int?>() != null;
        }

        return proposal;
    }

    public async Task<IEnumerable<VerifierLotteryParticipantEntryQm>> GetVerifierLotteryParticipants(
        Guid proposalId
    )
    {
        var dbConn = await _getOpenConnection();
        var entries = await dbConn.QueryAsync<VerifierLotteryParticipantEntryQm>(
            @"
                SELECT ""L1BlockNumber"" AS ""JoinedBlockNumber"", ""UserId"", ""UserData"", ""Nonce""
                FROM truquest_events.""JoinedThingAssessmentVerifierLotteryEvents""
                WHERE ""SettlementProposalId"" = @SettlementProposalId
                ORDER BY ""BlockNumber"" DESC, ""TxnIndex"" DESC
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