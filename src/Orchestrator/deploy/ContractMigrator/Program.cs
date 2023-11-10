using Nethereum.HdWallet;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;

var wallet = new Wallet(Environment.GetEnvironmentVariable("MNEMONIC")!, seedPassword: null);

var web3 = new Web3(wallet.GetAccount(0), "http://localhost:9545");

var txnReceipt = await web3.Eth
    .GetContractTransactionHandler<ImportSettlementProposalAssessmentVerifierLotteryDataMessage>()
    .SendRequestAndWaitForReceiptAsync("0xf30A5359475787464FEd1a6378ca25972bD64f6e", new()
    {
        ThingProposalIds = new()
        {
            "0x7878787878787878787878787878787878787878787878787878787878787878".HexToByteArray(),
            "0x5656565656565656565656565656565656565656565656565656565656565656".HexToByteArray()
        },
        OrchestratorCommitments = new()
        {
            new Commitment
            {
                DataHash = "0x1234567812345678123456781234567812345678123456781234567812345678".HexToByteArray(),
                UserXorDataHash = "0x1234567812345678123456781234567812345678123456781237777777777777".HexToByteArray(),
                Block = 78
            },
            new Commitment
            {
                DataHash = "0x1234567812345678123456781234567812345678123458888888888888888888".HexToByteArray(),
                UserXorDataHash = "0x1234567812345678123456781234567812345699999999999999999999999999".HexToByteArray(),
                Block = 333
            }
        },
        Participants = new()
        {
            new()
            {
                "0x1234512345123451234512345123451234512345",
                "0x9898989898989898989898989898888888888888"
            },
            new()
            {
                "0x5432154321543215432154321543215432154321"
            }
        },
        Claimants = new()
        {
            new()
            {
                "0x8888888888888888888888888888888888888888",
            },
            new()
            {
                "0x1111111111111111111111111111111111111111",
                "0x2222222222222222222222222222222222222222",
                "0xAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            }
        },
        BlockNumbers = new()
        {
            new()
            {
                5,
                58,
                17
            },
            new()
            {
                22,
                98,
                15,
                899
            }
        }
    });

// var result = await web3.Eth
//     .GetContractQueryHandler<ExportThingIdToSettlementProposalMappingAsArraysOfKeysAndValuesMessage>()
//     .QueryAsync<ExportThingIdToSettlementProposalMappingAsArraysOfKeysAndValuesFunctionOutput>("0x32D41E4e24F97ec7D52e3c43F8DbFe209CBd0e4c");

// for (int i = 0; i < result.ThingIds.Count; ++i)
// {
//     Console.WriteLine($"Thing {new Guid(result.ThingIds[i])}: Proposal {new Guid(result.SettlementProposals[i].Id)}");
// }

// var txnReceipt = await 

var otherRes = await web3.Eth
    .GetContractQueryHandler<ExportSettlementProposalAssessmentVerifierLotteryDataMessage>()
    .QueryAsync<ExportSettlementProposalAssessmentVerifierLotteryDataFunctionOutput>("0xf30A5359475787464FEd1a6378ca25972bD64f6e");

for (int i = 0; i < otherRes.ThingProposalIds.Count; ++i)
{
    Console.WriteLine(
        $"{otherRes.ThingProposalIds[i].ToHex(prefix: true)}: {otherRes.OrchestratorCommitments[i].DataHash.ToHex(prefix: true)}"
    );
    int k = 0;
    for (int j = 0; j < otherRes.Participants[i].Count; ++j)
    {
        Console.WriteLine($"\t\tParticipant {otherRes.Participants[i][j]}: {otherRes.BlockNumbers[i][k++]}");
    }
    for (int j = 0; j < otherRes.Claimants[i].Count; ++j)
    {
        Console.WriteLine($"\t\tClaimant {otherRes.Claimants[i][j]}: {otherRes.BlockNumbers[i][k++]}");
    }
}

// using var f = new FileStream("artifacts/TruQuest.bin", FileMode.Open, FileAccess.Read);
// using var r = new StreamReader(f);

// using var ff = new FileStream("artifacts/TruQuest-clone.bin", FileMode.CreateNew, FileAccess.Write);
// using var w = new StreamWriter(ff);
// await w.WriteAsync("**************************************");
// await w.WriteAsync(await r.ReadToEndAsync());
// await w.WriteAsync("**************************************");
// await w.FlushAsync();
