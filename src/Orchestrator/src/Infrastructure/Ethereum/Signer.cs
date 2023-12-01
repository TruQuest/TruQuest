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
using Application.Thing.Commands.CastValidationPollVote;
using Application.Settlement.Commands.CastAssessmentPollVote;

using Infrastructure.Ethereum.TypedData;

namespace Infrastructure.Ethereum;

internal class Signer : ISigner
{
    private readonly EthereumMessageSigner _personalSigner;
    private readonly Eip712TypedDataSigner _eip712Signer;
    private readonly DomainWithSalt _domain;
    private readonly EthECKey _orchestratorPrivateKey;
    private readonly string _orchestratorAddress;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public Signer(
        IConfiguration configuration,
        IAccountProvider accountProvider,
        EthereumMessageSigner personalSigner,
        Eip712TypedDataSigner eip712Signer
    )
    {
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

    public string RecoverFromNewThingValidationPollVoteMessage(
        NewThingValidationPollVoteIm input, string signature
    ) => _personalSigner.EncodeUTF8AndEcRecover(
            input.ToMessageForSigning(),
            signature
        );

    public string RecoverFromNewSettlementProposalAssessmentPollVoteMessage(
        NewSettlementProposalAssessmentPollVoteIm input, string signature
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

    public string SignNewThingValidationPollVote(
        NewThingValidationPollVoteIm input, string userId, string walletAddress,
        string signerAddress, string signature
    )
    {
        var td = new SignedNewThingValidationPollVoteTd
        {
            Vote = input.ToMessageForSigning(),
            UserId = userId,
            WalletAddress = walletAddress,
            SignerAddress = signerAddress,
            Signature = signature
        };

        return _personalSigner.EncodeUTF8AndSign(
            JsonSerializer.Serialize(td, _jsonSerializerOptions),
            _orchestratorPrivateKey
        );
    }

    public string SignNewSettlementProposalAssessmentPollVote(
        NewSettlementProposalAssessmentPollVoteIm input, string userId, string walletAddress,
        string signerAddress, string signature
    )
    {
        var td = new SignedNewSettlementProposalAssessmentPollVoteTd
        {
            Vote = input.ToMessageForSigning(),
            UserId = userId,
            WalletAddress = walletAddress,
            SignerAddress = signerAddress,
            Signature = signature
        };

        return _personalSigner.EncodeUTF8AndSign(
            JsonSerializer.Serialize(td, _jsonSerializerOptions),
            _orchestratorPrivateKey
        );
    }

    public string SignThingValidationPollVoteAgg(
        Guid thingId,
        ulong l1EndBlock,
        IEnumerable<ThingValidationPollVote> offChainVotes,
        IEnumerable<CastedThingValidationPollVoteEvent> onChainVotes
    )
    {
        var td = new SignedThingValidationPollVoteAggTd // @@TODO: This stuff ain't TypedData!
        {
            ThingId = thingId,
            L1EndBlock = l1EndBlock,
            OffChainVotes = offChainVotes
                .Select(v => new OffChainThingValidationPollVoteTd
                {
                    IpfsCid = v.IpfsCid
                })
                .ToList(),
            OnChainVotes = onChainVotes
                .Select(v => new OnChainThingValidationPollVoteTd
                {
                    TxnHash = v.TxnHash,
                    BlockNumber = v.BlockNumber,
                    TxnIndex = v.TxnIndex,
                    LogIndex = v.LogIndex, // @@NOTE: It's possible though unlikely that user voted twice in the same txn (thanks to AA).
                    UserId = v.UserId!,
                    WalletAddress = v.WalletAddress,
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

    public string SignSettlementProposalAssessmentPollVoteAgg(
        Guid thingId, Guid proposalId, ulong l1EndBlock,
        IEnumerable<SettlementProposalAssessmentPollVote> offChainVotes,
        IEnumerable<CastedSettlementProposalAssessmentPollVoteEvent> onChainVotes
    )
    {
        var td = new SignedSettlementProposalAssessmentPollVoteAggTd
        {
            ThingId = thingId,
            SettlementProposalId = proposalId,
            L1EndBlock = l1EndBlock,
            OffChainVotes = offChainVotes
                .Select(v => new OffChainSettlementProposalAssessmentPollVoteTd
                {
                    IpfsCid = v.IpfsCid
                })
                .ToList(),
            OnChainVotes = onChainVotes
                .Select(v => new OnChainSettlementProposalAssessmentPollVoteTd
                {
                    TxnHash = v.TxnHash,
                    BlockNumber = v.BlockNumber,
                    TxnIndex = v.TxnIndex,
                    LogIndex = v.LogIndex,
                    UserId = v.UserId!,
                    WalletAddress = v.WalletAddress,
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

    public string RecoverFromMessage(string message, string signature) =>
        _personalSigner.EncodeUTF8AndEcRecover(message, signature);
}
