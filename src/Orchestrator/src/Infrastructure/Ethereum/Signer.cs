using System.Text.Json;
using System.Text.Encodings.Web;

using Microsoft.Extensions.Configuration;

using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer.EIP712;
using Nethereum.Signer;

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
    private readonly JsonSerializerOptions _jsonSerializerOptions;

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

        _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
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

    public string RecoverFromNewAcceptancePollVoteMessage(
        NewAcceptancePollVoteIm input, string signature
    ) => _personalSigner.EncodeUTF8AndEcRecover(
            input.ToMessageForSigning(),
            signature
        );

    public string RecoverFromNewAssessmentPollVoteMessage(
        NewAssessmentPollVoteIm input, string signature
    ) => _personalSigner.EncodeUTF8AndEcRecover(
            input.ToMessageForSigning(),
            signature
        );

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

    public string SignNewAcceptancePollVote(
        NewAcceptancePollVoteIm input, string walletAddress,
        string ownerAddress, string ownerSignature
    )
    {
        var td = new SignedNewAcceptancePollVoteTd
        {
            Vote = input.ToMessageForSigning(),
            WalletAddress = walletAddress,
            OwnerAddress = ownerAddress,
            OwnerSignature = ownerSignature
        };

        return _personalSigner.EncodeUTF8AndSign(
            JsonSerializer.Serialize(td, _jsonSerializerOptions),
            _orchestratorPrivateKey
        );
    }

    public string SignNewAssessmentPollVote(
        NewAssessmentPollVoteIm input, string walletAddress,
        string ownerAddress, string ownerSignature
    )
    {
        var td = new SignedNewAssessmentPollVoteTd
        {
            Vote = input.ToMessageForSigning(),
            WalletAddress = walletAddress,
            OwnerAddress = ownerAddress,
            OwnerSignature = ownerSignature
        };

        return _personalSigner.EncodeUTF8AndSign(
            JsonSerializer.Serialize(td, _jsonSerializerOptions),
            _orchestratorPrivateKey
        );
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
            ThingId = thingId,
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

        return _personalSigner.EncodeUTF8AndSign(
            JsonSerializer.Serialize(td, _jsonSerializerOptions),
            _orchestratorPrivateKey
        );
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
            ThingId = thingId,
            SettlementProposalId = proposalId,
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
                    UserId = v.UserId, // @@TODO: EIP-55 encode
                    Decision = v.Decision.GetString(),
                    Reason = v.Reason ?? string.Empty
                })
                .ToList()
        };

        return _personalSigner.EncodeUTF8AndSign(
            JsonSerializer.Serialize(td, _jsonSerializerOptions),
            _orchestratorPrivateKey
        );
    }

    public bool CheckIsOrchestrator(String address) =>
        _accountProvider.LookupNameByAddress(address) == "Orchestrator";

    public string RecoverFromSiweMessage(string message, string signature) =>
        _personalSigner.EncodeUTF8AndEcRecover(message, signature);
}
