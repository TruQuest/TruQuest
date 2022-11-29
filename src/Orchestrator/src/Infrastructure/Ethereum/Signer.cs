using Microsoft.Extensions.Configuration;

using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer.EIP712;
using Nethereum.Signer;

using Domain.Errors;
using Domain.Results;
using Application.Account.Commands.SignUp;
using Application.Common.Interfaces;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;

using Infrastructure.Ethereum.TypedData;

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

    public Either<AccountError, string> RecoverFromSignUpMessage(SignUpIm input, string signature)
    {
        var td = new SignUpTd { Username = input.Username };
        var tdDefinition = _getTypedDataDefinition(typeof(SignUpTd));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address.Replace("0x", string.Empty);
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

        return address.Replace("0x", string.Empty);
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

        return address.Replace("0x", string.Empty);
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
}