using Microsoft.Extensions.Configuration;

using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer.EIP712;
using Nethereum.Signer;

using Domain.Errors;
using Domain.Results;
using Domain.Aggregates.Events;
using Domain.Aggregates;
using Application.Common.Interfaces;
using Application.Thing.Commands.CastAcceptancePollVote;
using Application.Settlement.Commands.CastAssessmentPollVote;

using Infrastructure.Ethereum.TypedData;

namespace Infrastructure.Ethereum;

internal class Signer : ISigner
{
    private readonly AccountProvider _accountProvider;
    private readonly EthereumMessageSigner _personalSigner;
    private readonly Eip712TypedDataSigner _eip712Signer;
    private readonly DomainWithSalt _domain;
    private readonly EthECKey _orchestratorPrivateKey;
    private readonly string _orchestratorAddress;

    public Signer(
        IConfiguration configuration,
        AccountProvider accountProvider,
        EthereumMessageSigner personalSigner,
        Eip712TypedDataSigner eip712Signer
    )
    {
        _accountProvider = accountProvider;
        _personalSigner = personalSigner;
        _eip712Signer = eip712Signer;

        var network = configuration["Ethereum:Network"]!;
        var domainConfig = configuration.GetSection("Ethereum:Domain");
        _domain = new()
        {
            Name = domainConfig["Name"],
            Version = domainConfig["Version"],
            ChainId = configuration.GetValue<int>($"Ethereum:Networks:{network}:ChainId"),
            VerifyingContract = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"],
            Salt = domainConfig["Salt"].HexToByteArray()
        };

        var orchestrator = accountProvider.GetAccount("Orchestrator");
        _orchestratorPrivateKey = new EthECKey(orchestrator.PrivateKey);
        _orchestratorAddress = orchestrator.Address;
    }

    private TypedData<DomainWithSalt> _getTypedDataDefinition(params Type[] types)
    {
        var typeList = new List<Type>(types);
        typeList.Insert(0, typeof(DomainWithSalt));
        return new TypedData<DomainWithSalt>
        {
            Domain = _domain,
            Types = MemberDescriptionFactory.GetTypesMemberDescription(typeList.ToArray()),
            PrimaryType = types.First().Name,
        };
    }

    public Either<VoteError, string> RecoverFromNewAcceptancePollVoteMessage(
        NewAcceptancePollVoteIm input, string signature
    )
    {
        var td = new NewAcceptancePollVoteTd
        {
            ThingId = input.ThingId.ToString(),
            CastedAt = input.CastedAt,
            Decision = input.Decision.GetString(),
            Reason = input.Reason
        };
        var tdDefinition = _getTypedDataDefinition(typeof(NewAcceptancePollVoteTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address.Substring(2);
    }

    public Either<VoteError, string> RecoverFromNewAssessmentPollVoteMessage(
        NewAssessmentPollVoteIm input, string signature
    )
    {
        var td = new NewAssessmentPollVoteTd
        {
            ThingId = input.ThingId.ToString(),
            SettlementProposalId = input.SettlementProposalId.ToString(),
            CastedAt = input.CastedAt,
            Decision = input.Decision.GetString(),
            Reason = input.Reason
        };
        var tdDefinition = _getTypedDataDefinition(typeof(NewAssessmentPollVoteTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address.Substring(2);
    }

    public string SignThing(Guid thingId)
    {
        var td = new ThingTd
        {
            Id = thingId.ToByteArray()
        };
        var tdDefinition = _getTypedDataDefinition(typeof(ThingTd));
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public string SignNewAcceptancePollVote(NewAcceptancePollVoteIm input, string voterId, string voterSignature)
    {
        var td = new SignedNewAcceptancePollVoteTd
        {
            Vote = new NewAcceptancePollVoteTd
            {
                ThingId = input.ThingId.ToString(),
                CastedAt = input.CastedAt,
                Decision = input.Decision.GetString(),
                Reason = input.Reason
            },
            VoterId = voterId,
            VoterSignature = voterSignature
        };
        var tdDefinition = _getTypedDataDefinition(
            typeof(SignedNewAcceptancePollVoteTd),
            typeof(NewAcceptancePollVoteTd)
        );
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public string SignNewAssessmentPollVote(NewAssessmentPollVoteIm input, string voterId, string voterSignature)
    {
        var td = new SignedNewAssessmentPollVoteTd
        {
            Vote = new NewAssessmentPollVoteTd
            {
                ThingId = input.ThingId.ToString(),
                SettlementProposalId = input.SettlementProposalId.ToString(),
                CastedAt = input.CastedAt,
                Decision = input.Decision.GetString(),
                Reason = input.Reason
            },
            VoterId = voterId,
            VoterSignature = voterSignature
        };
        var tdDefinition = _getTypedDataDefinition(
            typeof(SignedNewAssessmentPollVoteTd),
            typeof(NewAssessmentPollVoteTd)
        );
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public string SignAcceptancePollVoteAgg(
        Guid thingId,
        ulong endBlock,
        IEnumerable<AcceptancePollVote> offChainVotes,
        IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes
    )
    {
        var td = new SignedAcceptancePollVoteAggTd
        {
            ThingId = thingId.ToString(),
            EndBlock = endBlock,
            OffChainVotes = offChainVotes
                .Select(v => new OffChainAcceptancePollVoteTd
                {
                    IpfsCid = v.IpfsCid
                })
                .ToList(),
            OnChainVotes = onChainVotes
                .Select(v => new OnChainAcceptancePollVoteTd
                {
                    BlockNumber = v.BlockNumber,
                    TxnIndex = v.TxnIndex,
                    L1BlockNumber = v.L1BlockNumber,
                    UserId = v.UserId, // @@TODO: EIP-55 encode
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty
                })
                .ToList()
        };
        var tdDefinition = _getTypedDataDefinition(
            typeof(SignedAcceptancePollVoteAggTd),
            typeof(OffChainAcceptancePollVoteTd),
            typeof(OnChainAcceptancePollVoteTd)
        );
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public string SignSettlementProposal(Guid thingId, Guid proposalId)
    {
        var td = new SettlementProposalTd
        {
            ThingId = thingId.ToByteArray(),
            Id = proposalId.ToByteArray()
        };
        var tdDefinition = _getTypedDataDefinition(typeof(SettlementProposalTd));
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public string SignAssessmentPollVoteAgg(
        Guid thingId, Guid proposalId, ulong endBlock,
        IEnumerable<AssessmentPollVote> offChainVotes,
        IEnumerable<CastedAssessmentPollVoteEvent> onChainVotes
    )
    {
        var td = new SignedAssessmentPollVoteAggTd
        {
            ThingId = thingId.ToString(),
            SettlementProposalId = proposalId.ToString(),
            EndBlock = endBlock,
            OffChainVotes = offChainVotes
                .Select(v => new OffChainAssessmentPollVoteTd
                {
                    IpfsCid = v.IpfsCid
                })
                .ToList(),
            OnChainVotes = onChainVotes
                .Select(v => new OnChainAssessmentPollVoteTd
                {
                    BlockNumber = v.BlockNumber,
                    TxnIndex = v.TxnIndex,
                    L1BlockNumber = v.L1BlockNumber,
                    UserId = v.UserId,
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty
                })
                .ToList()
        };
        var tdDefinition = _getTypedDataDefinition(
            typeof(SignedAssessmentPollVoteAggTd),
            typeof(OffChainAssessmentPollVoteTd),
            typeof(OnChainAssessmentPollVoteTd)
        );
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public bool CheckIsOrchestrator(String address) =>
        _accountProvider.LookupNameByAddress(address) == "Orchestrator";

    public string RecoverFromSiweMessage(string message, string signature) =>
        _personalSigner.EncodeUTF8AndEcRecover(message, signature);
}
