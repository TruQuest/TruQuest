using Microsoft.Extensions.Configuration;

using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer.EIP712;

using Domain.Errors;
using Domain.Results;
using Application.Account.Commands.SignUp;
using Application.Common.Interfaces;
using Application.Subject.Commands.AddNewSubject;

using Infrastructure.Ethereum.TypedData;

namespace Infrastructure.Ethereum;

internal class Signer : ISigner
{
    private readonly Eip712TypedDataSigner _eip712Signer;
    private readonly DomainWithSalt _domain;

    public Signer(IConfiguration configuration, Eip712TypedDataSigner eip712Signer)
    {
        _eip712Signer = eip712Signer;

        var domainConfig = configuration.GetSection("Ethereum:Domain");
        _domain = new()
        {
            Name = domainConfig["Name"],
            Version = domainConfig["Version"],
            ChainId = domainConfig.GetValue<int>("ChainId"),
            VerifyingContract = domainConfig["VerifyingContract"],
            Salt = domainConfig["Salt"].HexToByteArray()
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

    public Either<AccountError, string> RecoverFromSignUpMessage(SignUpIM input, string signature)
    {
        var td = new SignUpTD { Username = input.Username };
        var tdDefinition = _getTypedDataDefinition(typeof(SignUpTD));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address;
    }

    public Either<SubjectError, string> RecoverFromNewSubjectMessage(NewSubjectIM input, string signature)
    {
        var td = new NewSubjectTD
        {
            Type = (int)input.Type,
            Name = input.Name,
            Details = input.Details,
            ImageURL = input.ImageURL,
            Tags = input.Tags.Select(t => new TagTD { Id = t.Id }).ToList()
        };
        var tdDefinition = _getTypedDataDefinition(typeof(NewSubjectTD), typeof(TagTD));
        var address = _eip712Signer.RecoverFromSignatureV4(td, tdDefinition, signature);

        return address;
    }
}