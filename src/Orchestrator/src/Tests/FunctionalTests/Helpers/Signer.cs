using Microsoft.Extensions.Configuration;

using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

using Application.Subject.Commands.AddNewSubject;
using Infrastructure.Ethereum.TypedData;

namespace Tests.FunctionalTests.Helpers;

public class Signer
{
    private readonly Eip712TypedDataSigner _eip712Signer;
    private readonly DomainWithSalt _domain;
    private readonly EthECKey _playerPrivateKey;

    public Signer(IConfiguration configuration)
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

        _playerPrivateKey = new EthECKey(configuration[$"Ethereum:Accounts:{network}:Player:PrivateKey"]!);
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

        return _eip712Signer.SignTypedDataV4(tdDefinition, _playerPrivateKey);
    }
}