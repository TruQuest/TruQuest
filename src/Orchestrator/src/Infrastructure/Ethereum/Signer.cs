using Microsoft.Extensions.Configuration;

using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer.EIP712;
using Nethereum.Signer;

using Domain.Errors;
using Domain.Results;
using Domain.Aggregates.Events;
using Domain.Aggregates;
using Application.User.Commands.SignUp;
using Application.Common.Interfaces;
using Application.Thing.Commands.CastAcceptancePollVote;

using Infrastructure.Ethereum.TypedData;

namespace Infrastructure.Ethereum;

internal class Signer : ISigner
{
    private readonly Eip712TypedDataSigner _eip712Signer;
    private readonly DomainWithSalt _domain;
    private readonly EthECKey _orchestratorPrivateKey;
    private readonly string _orchestratorAddress;

    public Signer(
        IConfiguration configuration,
        AccountProvider accountProvider,
        Eip712TypedDataSigner eip712Signer
    )
    {
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

    public Either<UserError, string> RecoverFromSignUpMessage(SignUpIm input, string signature)
    {
        var td = new SignUpTd { Username = input.Username };
        var tdDefinition = _getTypedDataDefinition(typeof(SignUpTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address.Substring(2);
    }

    public Either<VoteError, string> RecoverFromNewAcceptancePollVoteMessage(NewAcceptancePollVoteIm input, string signature)
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

    public bool CheckOrchestratorSignatureOnTimestamp(string timestamp, string signature)
    {
        var td = new TimestampTd
        {
            Timestamp = timestamp
        };
        var tdDefinition = _getTypedDataDefinition(typeof(TimestampTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address == _orchestratorAddress;
    }

    public string RecoverFromSignInMessage(string timestamp, string orchestratorSignature, string signature)
    {
        var td = new SignInTd
        {
            Timestamp = timestamp,
            OrchestratorSignature = orchestratorSignature
        };
        var tdDefinition = _getTypedDataDefinition(typeof(SignInTd));
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

    public string SignAcceptancePollVoteAgg(
        Guid thingId,
        IEnumerable<AcceptancePollVote> offChainVotes,
        IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes
    )
    {
        var td = new SignedAcceptancePollVoteAggTd
        {
            ThingId = thingId.ToString(),
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
                    UserId = "0x" + v.UserId, // @@TODO: EIP-55 encode
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
        IEnumerable<AssessmentPollVote> offChainVotes, IEnumerable<CastedAssessmentPollVoteEvent> onChainVotes
    )
    {
        var td = new SignedAssessmentPollVoteAggTd
        {
            OffChainVotes = offChainVotes
                .Select(v => new OffChainAssessmentPollVoteTd
                {
                    SettlementProposalId = v.SettlementProposalId.ToString(),
                    VoterId = "0x" + v.VoterId,
                    PollType = "Assessment",
                    CastedAt = DateTimeOffset.FromUnixTimeMilliseconds(v.CastedAtMs).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty,
                    IpfsCid = v.IpfsCid,
                    VoterSignature = v.VoterSignature
                })
                .ToList(),
            OnChainVotes = onChainVotes
                .Select(v => new OnChainAssessmentPollVoteTd
                {
                    BlockNumber = v.BlockNumber,
                    TxnIndex = v.TxnIndex,
                    SettlementProposalId = v.SettlementProposalId.ToString(),
                    UserId = "0x" + v.UserId,
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

    public string SignTimestamp(DateTimeOffset timestamp)
    {
        var td = new TimestampTd
        {
            Timestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
        var tdDefinition = _getTypedDataDefinition(typeof(TimestampTd));
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }
}