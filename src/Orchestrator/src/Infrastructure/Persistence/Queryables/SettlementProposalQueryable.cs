using Microsoft.EntityFrameworkCore;

using Dapper;

using Domain.Aggregates;
using Application.Thing.Queries.GetSettlementProposalsList;
using Application.Common.Interfaces;
using Application.Settlement.Queries.GetSettlementProposal;
using Application.Admin.Queries.GetContractsStates.QM;

namespace Infrastructure.Persistence.Queryables;

internal class SettlementProposalQueryable : Queryable, ISettlementProposalQueryable
{
    private new readonly AppDbContext _dbContext;

    public SettlementProposalQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SettlementProposalPreviewQm>> GetForThing(Guid thingId, string? userId)
    {
        var proposals = await _dbContext.SettlementProposals
            .AsNoTracking()
            .Where(p => p.ThingId == thingId && (p.State != SettlementProposalState.Draft || p.SubmitterId == userId))
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
                    p.*, u.""WalletAddress"" AS ""SubmitterWalletAddress"",
                    s.""Name"" AS ""SubjectName"",
                    t.""Title"" AS ""ThingTitle"", t.""CroppedImageIpfsCid"" AS ""ThingCroppedImageIpfsCid"",
                    e.*
                FROM
                    truquest.""SettlementProposals"" AS p
                        INNER JOIN
                    truquest.""AspNetUsers"" AS u
                        ON p.""SubmitterId"" = u.""Id""
                        INNER JOIN
                    truquest.""Things"" AS t
                        ON p.""ThingId"" = t.""Id""
                        INNER JOIN
                    truquest.""Subjects"" AS s
                        ON t.""SubjectId"" = s.""Id""
                        INNER JOIN
                    truquest.""SettlementProposalEvidence"" AS e
                        ON p.""Id"" = e.""SettlementProposalId""
                WHERE p.""Id"" = @ItemId;

                SELECT 1
                FROM truquest.""WatchList""
                WHERE
                    (""UserId"", ""ItemType"", ""ItemId"", ""ItemUpdateCategory"") =
                    (@UserId, @ItemType::truquest.watched_item_type, @ItemId, @ItemUpdateCategory);
            ",
            param: new
            {
                UserId = userId,
                ItemType = WatchedItemType.SettlementProposal.GetString(),
                ItemId = id,
                ItemUpdateCategory = (int)SettlementProposalUpdateCategory.General
            }
        );

        var proposal = multiQuery.SingleWithMany<SettlementProposalQm, SettlementProposalEvidenceQm>(
            joinedCollectionSelector: proposal => proposal.Evidence
        );
        if (proposal != null)
        {
            proposal.Watched = multiQuery.ReadSingleOrDefault<int?>() != null;
        }

        return proposal;
    }

    public Task<List<SettlementProposalTitleAndThingInfoQm>> GetTitleAndThingInfoFor(IEnumerable<Guid> proposalIds) =>
        _dbContext.SettlementProposals
            .Where(p => proposalIds.Contains(p.Id))
            .Select(p => new SettlementProposalTitleAndThingInfoQm
            {
                Id = p.Id,
                Title = p.Title,
                ThingId = p.ThingId
            })
            .ToListAsync();
}
