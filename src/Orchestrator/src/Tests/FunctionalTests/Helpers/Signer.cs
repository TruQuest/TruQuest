using Nethereum.Signer;

using Application.Thing.Commands.CastValidationPollVote;
using Application.Settlement.Commands.CastAssessmentPollVote;
using Infrastructure.Ethereum;

namespace Tests.FunctionalTests.Helpers;

public class Signer
{
    private readonly EthereumMessageSigner _personalSigner;
    private readonly AccountProvider _accountProvider;

    public Signer(AccountProvider accountProvider)
    {
        _personalSigner = new();
        _accountProvider = accountProvider;
    }

    public string SignNewThingValidationPollVoteMessageAs(string accountName, NewThingValidationPollVoteIm input) =>
        _personalSigner.EncodeUTF8AndSign(
            input.ToMessageForSigning(),
            new EthECKey(_accountProvider.GetAccount(accountName).PrivateKey)
        );

    public string SignNewSettlementProposalAssessmentPollVoteMessageAs(
        string accountName, NewSettlementProposalAssessmentPollVoteIm input
    ) => _personalSigner.EncodeUTF8AndSign(
        input.ToMessageForSigning(),
        new EthECKey(_accountProvider.GetAccount(accountName).PrivateKey)
    );
}
