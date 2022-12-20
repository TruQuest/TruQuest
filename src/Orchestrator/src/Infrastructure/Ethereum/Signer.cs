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
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;
using Application.Vote.Commands.CastVote;

using Infrastructure.Ethereum.TypedData;
using Application.Settlement.Commands.SubmitNewSettlementProposal;

namespace Infrastructure.Ethereum;

internal class Signer : ISigner
{
    private readonly Eip712TypedDataSigner _eip712Signer;
    private readonly DomainWithSalt _domain;
    private readonly EthECKey _orchestratorPrivateKey;

    public Signer(IConfiguration configuration, Eip712TypedDataSigner eip712Signer)
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

        _orchestratorPrivateKey = new EthECKey(configuration[$"Ethereum:Accounts:{network}:Orchestrator:PrivateKey"]!);
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

    public Either<SubjectError, string> RecoverFromNewSubjectMessage(NewSubjectIm input, string signature)
    {
        var td = new NewSubjectTd
        {
            Type = (int)input.Type,
            Name = input.Name,
            Details = input.Details,
            ImageUrl = input.ImageUrl,
            Tags = input.Tags.Select(t => new TagTd { Id = t.Id }).ToList()
        };
        var tdDefinition = _getTypedDataDefinition(typeof(NewSubjectTd), typeof(TagTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address.Substring(2);
    }

    public Either<ThingError, string> RecoverFromNewThingMessage(NewThingIm input, string signature)
    {
        var td = new NewThingTd
        {
            SubjectId = input.SubjectId.ToString(),
            Title = input.Title,
            Details = input.Details,
            ImageUrl = input.ImageUrl,
            Evidence = input.Evidence.Select(e => new EvidenceTd { Url = e.Url }).ToList(),
            Tags = input.Tags.Select(t => new TagTd { Id = t.Id }).ToList()
        };
        var tdDefinition = _getTypedDataDefinition(typeof(NewThingTd), typeof(EvidenceTd), typeof(TagTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address.Substring(2);
    }

    public Either<VoteError, string> RecoverFromNewVoteMessage(NewVoteIm input, string signature)
    {
        var td = new NewVoteTd
        {
            ThingId = input.ThingId.ToString(),
            PollType = input.PollType.GetString(),
            CastedAt = input.CastedAt,
            Decision = input.Decision.GetString(),
            Reason = input.Reason
        };
        var tdDefinition = _getTypedDataDefinition(typeof(NewVoteTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address.Substring(2);
    }

    public Either<SettlementError, string> RecoverFromNewSettlementProposalMessage(
        NewSettlementProposalIm input, string signature
    )
    {
        var td = new NewSettlementProposalTd
        {
            ThingId = input.ThingId.ToString(),
            Title = input.Title,
            Verdict = (int)input.Verdict,
            Details = input.Details,
            Evidence = input.Evidence.Select(e => new SupportingEvidenceTd { Url = e.Url }).ToList()
        };
        var tdDefinition = _getTypedDataDefinition(typeof(NewSettlementProposalTd), typeof(SupportingEvidenceTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address.Substring(2);
    }

    public string SignThing(ThingVm thing)
    {
        var td = new ThingTd
        {
            Id = thing.Id
        };
        var tdDefinition = _getTypedDataDefinition(typeof(ThingTd));
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public string SignNewVote(NewVoteIm input, string voterId, string voterSignature)
    {
        var td = new SignedNewVoteTd
        {
            Vote = new NewVoteTd
            {
                ThingId = input.ThingId.ToString(),
                PollType = input.PollType.GetString(),
                CastedAt = input.CastedAt,
                Decision = input.Decision.GetString(),
                Reason = input.Reason
            },
            VoterId = voterId,
            VoterSignature = voterSignature
        };
        var tdDefinition = _getTypedDataDefinition(typeof(SignedNewVoteTd), typeof(NewVoteTd));
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public string SignVoteAgg(
        IEnumerable<Vote> offChainVotes, IEnumerable<CastedAcceptancePollVoteEvent> onChainVotes
    )
    {
        var td = new SignedVoteAggTd
        {
            OffChainVotes = offChainVotes
                .Select(v => new OffChainVoteTd
                {
                    ThingId = v.ThingId.ToString(),
                    VoterId = "0x" + v.VoterId,
                    PollType = v.PollType.GetString(),
                    CastedAt = DateTimeOffset.FromUnixTimeMilliseconds(v.CastedAtMs).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty,
                    IpfsCid = v.IpfsCid,
                    VoterSignature = v.VoterSignature
                })
                .ToList(),
            OnChainVotes = onChainVotes
                .Select(v => new OnChainVoteTd
                {
                    BlockNumber = v.BlockNumber,
                    TxnIndex = v.TxnIndex,
                    ThingIdHash = v.ThingIdHash,
                    UserId = "0x" + v.UserId,
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty
                })
                .ToList()
        };
        var tdDefinition = _getTypedDataDefinition(
            typeof(SignedVoteAggTd), typeof(OffChainVoteTd), typeof(OnChainVoteTd)
        );
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }

    public string SignSettlementProposal(SettlementProposalVm proposal)
    {
        var td = new SettlementProposalTd
        {
            ThingId = proposal.ThingId,
            Id = proposal.Id
        };
        var tdDefinition = _getTypedDataDefinition(typeof(SettlementProposalTd));
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _orchestratorPrivateKey);
    }
}