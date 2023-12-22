using GoThataway;

using Domain.Results;
using Domain.Aggregates;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.General.Queries.GetContractsStates.VM;
using Application.General.Queries.GetContractsStates.QM;

namespace Application.General.Queries.GetContractsStates;

[RequireAuthorization(Policy = "AdminOnly")]
public class GetContractsStatesQuery : IRequest<HandleResult<GetContractsStatesRvm>> { }

public class GetContractsStatesQueryHandler : IRequestHandler<GetContractsStatesQuery, HandleResult<GetContractsStatesRvm>>
{
    private readonly IContractCaller _contractCaller;
    private readonly IUserRepository _userRepository;
    private readonly IThingQueryable _thingQueryable;
    private readonly ISettlementProposalQueryable _settlementProposalQueryable;

    public GetContractsStatesQueryHandler(
        IContractCaller contractCaller,
        IUserRepository userRepository,
        IThingQueryable thingQueryable,
        ISettlementProposalQueryable settlementProposalQueryable
    )
    {
        _contractCaller = contractCaller;
        _userRepository = userRepository;
        _thingQueryable = thingQueryable;
        _settlementProposalQueryable = settlementProposalQueryable;
    }

    public async Task<HandleResult<GetContractsStatesRvm>> Handle(GetContractsStatesQuery query, CancellationToken ct)
    {
        var truQuestInfo = await _contractCaller.ExportTruQuestContractInfo();

        var whitelistedAddresses = await _contractCaller.GetRestrictedAccessWhitelist();
        var userBalances = await _contractCaller.ExportUsersAndBalances();

        // @@TODO: Use queryable instead of repo.
        var users = await _userRepository.GetUserIdsForWalletAddresses(userBalances.Select(u => u.Address).ToList());

        var thingSubmitters = await _contractCaller.ExportThingSubmitter();
        var things = await _thingQueryable.GetTitleAndSubjectInfoFor(thingSubmitters.Select(t => t.ThingId).ToList());

        var settlementProposalSubmitters = await _contractCaller.ExportThingIdToSettlementProposal();
        var settlementProposals = await _settlementProposalQueryable.GetTitleAndThingInfoFor(
            settlementProposalSubmitters.Select(s => s.SettlementProposalId).ToList()
        );

        var thingValidationVerifierLotteries = await _contractCaller.ExportThingValidationVerifierLotteryData();
        var thingValidationPolls = await _contractCaller.ExportThingValidationPollData();

        var settlementProposalAssessmentVerifierLotteries = await _contractCaller.ExportSettlementProposalAssessmentVerifierLotteryData();
        var settlementProposalAssessmentPolls = await _contractCaller.ExportSettlementProposalAssessmentPollData();

        var thingsGroupedBySubject = things.GroupBy(t => t.SubjectId);
        var subjects = new List<SubjectVm>(thingsGroupedBySubject.Count());
        foreach (var group in thingsGroupedBySubject)
        {
            var subjectThings = new List<ThingVm>(group.Count());
            var subject = new SubjectVm
            {
                Id = group.Key,
                Name = group.First().SubjectName,
                Things = subjectThings
            };

            foreach (var thing in group)
            {
                var lottery = thingValidationVerifierLotteries.SingleOrDefault(l => l.ThingId == thing.Id);
                var poll = thingValidationPolls.SingleOrDefault(p => p.ThingId == thing.Id);
                var proposal = settlementProposals.SingleOrDefault(p => p.ThingId == thing.Id);
                SettlementProposalAssessmentVerifierLotteryQm? proposalLottery = null;
                SettlementProposalAssessmentPollQm? proposalPoll = null;
                if (proposal != null)
                {
                    proposalLottery = settlementProposalAssessmentVerifierLotteries.SingleOrDefault(l => l.SettlementProposalId == proposal.Id);
                    proposalPoll = settlementProposalAssessmentPolls.SingleOrDefault(p => p.SettlementProposalId == proposal.Id);
                }

                subjectThings.Add(new()
                {
                    Id = thing.Id,
                    Title = thing.Title,
                    Lottery = lottery != null ?
                        new()
                        {
                            OrchestratorCommitment = lottery.OrchestratorCommitment,
                            Participants = lottery.Participants.Select(p => new LotteryParticipantVm
                            {
                                UserId = users.Single(u => u.WalletAddress == p.Address).UserId,
                                WalletAddress = p.Address,
                                JoinedBlockNumber = p.BlockNumber
                            })
                        } : null,
                    Poll = poll != null ?
                        new()
                        {
                            InitBlockNumber = poll.InitBlockNumber,
                            Verifiers = poll.Verifiers.Select(v => new VerifierVm
                            {
                                UserId = users.Single(u => u.WalletAddress == v).UserId,
                                WalletAddress = v
                            })
                        } : null,
                    SettlementProposal = proposal != null ?
                        new()
                        {
                            Id = proposal.Id,
                            Title = proposal.Title,
                            Lottery = proposalLottery != null ? new()
                            {
                                OrchestratorCommitment = proposalLottery.OrchestratorCommitment,
                                Claimants = proposalLottery.Claimants.Select(c => new LotteryParticipantVm
                                {
                                    UserId = users.Single(u => u.WalletAddress == c.Address).UserId,
                                    WalletAddress = c.Address,
                                    JoinedBlockNumber = c.BlockNumber
                                }),
                                Participants = proposalLottery.Participants.Select(p => new LotteryParticipantVm
                                {
                                    UserId = users.Single(u => u.WalletAddress == p.Address).UserId,
                                    WalletAddress = p.Address,
                                    JoinedBlockNumber = p.BlockNumber
                                }),
                            } : null,
                            Poll = proposalPoll != null ?
                                new()
                                {
                                    InitBlockNumber = proposalPoll.InitBlockNumber,
                                    Verifiers = proposalPoll.Verifiers.Select(v => new VerifierVm
                                    {
                                        UserId = users.Single(u => u.WalletAddress == v).UserId,
                                        WalletAddress = v
                                    })
                                } : null
                        } : null
                });
            }

            subjects.Add(subject);
        }

        return new()
        {
            Data = new()
            {
                TruQuestInfo = truQuestInfo,
                WhitelistedWalletAddresses = whitelistedAddresses,
                Users = users.Join(userBalances, u => u.WalletAddress, ub => ub.Address, (u, ub) => new UserVm
                {
                    Id = u.UserId,
                    WalletAddress = u.WalletAddress,
                    HexBalance = ub.HexBalance,
                    HexStakedBalance = ub.HexStakedBalance
                }),
                Subjects = subjects
            }
        };
    }
}
