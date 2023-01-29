using Microsoft.Extensions.Configuration;

using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

using Application.Thing.Commands.CastAcceptancePollVote;
using Infrastructure.Ethereum;
using Infrastructure.Ethereum.TypedData;

namespace Tests.FunctionalTests.Helpers;

public class Signer
{
    private readonly Eip712TypedDataSigner _eip712Signer;
    private readonly DomainWithSalt _domain;
    private readonly AccountProvider _accountProvider;

    public Signer(IConfiguration configuration, AccountProvider accountProvider)
    {
        _eip712Signer = new Eip712TypedDataSigner();
        _accountProvider = accountProvider;

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

    public string SignNewAcceptancePollVoteMessageAs(string accountName, NewAcceptancePollVoteIm input)
    {
        var privateKey = new EthECKey(_accountProvider.GetAccount(accountName).PrivateKey);
        var td = new NewAcceptancePollVoteTd
        {
            ThingId = input.ThingId.ToString(),
            CastedAt = input.CastedAt,
            Decision = input.Decision.GetString(),
            Reason = input.Reason
        };
        var tdDefinition = _getTypedDataDefinition(typeof(NewAcceptancePollVoteTd));
        tdDefinition.SetMessage(td);

        return _eip712Signer.SignTypedDataV4(tdDefinition, privateKey);
    }
}