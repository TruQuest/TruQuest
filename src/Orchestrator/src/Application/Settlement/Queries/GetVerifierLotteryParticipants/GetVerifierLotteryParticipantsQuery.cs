using System.Diagnostics;

using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsQuery : IRequest<HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    public required Guid ProposalId { get; init; }
}

internal class Validator : AbstractValidator<GetVerifierLotteryParticipantsQuery>
{
    public Validator()
    {
        RuleFor(q => q.ProposalId).NotEmpty();
    }
}

internal class GetVerifierLotteryParticipantsQueryHandler :
    IRequestHandler<GetVerifierLotteryParticipantsQuery, HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    private readonly ISettlementProposalQueryable _settlementProposalQueryable;
    private readonly ISigner _signer;

    public GetVerifierLotteryParticipantsQueryHandler(
        ISettlementProposalQueryable settlementProposalQueryable,
        ISigner signer
    )
    {
        _settlementProposalQueryable = settlementProposalQueryable;
        _signer = signer;
    }

    public async Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> Handle(
        GetVerifierLotteryParticipantsQuery query, CancellationToken ct
    )
    {
        // ordered from newest to oldest
        var (participants, claimants) = await _settlementProposalQueryable.GetVerifierLotteryParticipants(query.ProposalId);
        var latestParticipant = participants.FirstOrDefault();
        if (latestParticipant != null && _signer.CheckIsOrchestrator(latestParticipant.UserId))
        {
            // means the lottery was closed with success
            Debug.Assert(latestParticipant.Nonce != null && latestParticipant.UserData == null);
            latestParticipant.MarkAsOrchestrator();

            var verifierIds = await _settlementProposalQueryable.GetVerifiers(query.ProposalId);
            Debug.Assert(verifierIds.Any());

            var participantsAndClaimants = participants.Concat(claimants).ToHashSet();
            // there should be no users that are both participant and claimant
            Debug.Assert(participantsAndClaimants.Count() == participants.Count() + claimants.Count());
            foreach (var verifierId in verifierIds)
            {
                var entry = participantsAndClaimants.Single(e => e.UserId == verifierId);
                entry.MarkAsWinner();
            }
        }
        else
        {
            // @@NOTE: There is a period when the lottery is closed (and therefore the nonces are set)
            // but not yet finalized (verifiers are not yet added to the db, etc.). So we just clear
            // nonces here since it would be confusing to already show nonces but not winners. 
            var entry = participants.FirstOrDefault() ?? claimants.FirstOrDefault();
            if (entry?.Nonce != null)
            {
                // if one entry's nonce is set then all entries' nonces are set since we set them in a txn
                foreach (var e in participants.Concat(claimants)) e.ClearSensitiveData();
            }
        }

        participants = participants
            .OrderBy(e => e.SortKey)
            .ThenByDescending(e => e.BlockNumber);

        claimants = claimants
            .OrderBy(e => e.SortKey)
            .ThenByDescending(e => e.BlockNumber);

        return new()
        {
            Data = new()
            {
                ProposalId = query.ProposalId,
                Participants = participants,
                Claimants = claimants
            }
        };
    }
}
