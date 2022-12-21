using Microsoft.Extensions.Configuration;

using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;
using Application.Vote.Commands.CastVote;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using Infrastructure.Ethereum;
using Infrastructure.Ethereum.TypedData;

namespace Tests.FunctionalTests.Helpers;

public class Signer
{
    private readonly Eip712TypedDataSigner _eip712Signer;
    private readonly DomainWithSalt _domain;
    private readonly EthECKey _submitterPrivateKey;
    private readonly EthECKey _proposerPrivateKey;

    public Signer(IConfiguration configuration, AccountProvider accountProvider)
    {
        _eip712Signer = new Eip712TypedDataSigner();

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

        _submitterPrivateKey = new EthECKey(accountProvider.GetAccount("Submitter").PrivateKey);
        _proposerPrivateKey = new EthECKey(accountProvider.GetAccount("Proposer").PrivateKey);
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

    public string SignNewSubjectMessage(NewSubjectIm input)
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
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _submitterPrivateKey);
    }

    public string SignNewThingMessage(NewThingIm input)
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
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _submitterPrivateKey);
    }

    public string SignNewVoteMessage(NewVoteIm input)
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
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _submitterPrivateKey);
    }

    public string SignNewSettlementProposalMessage(NewSettlementProposalIm input)
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
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, _proposerPrivateKey);
    }
}