using System.Numerics;
using System.Text.Json;

using Nethereum.HdWallet;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")!;
var rpcUrl = Environment.GetEnvironmentVariable("ETHEREUM_RPC_URL")!;
Account orchestrator;
if (environment == "Development")
{
    var faucet = new Account("0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80");
    var web3Faucet = new Web3(faucet, rpcUrl);

    var wallet = new Wallet(Environment.GetEnvironmentVariable("MNEMONIC")!, seedPassword: null);
    var accountFactoryAddress = Environment.GetEnvironmentVariable("ACCOUNT_FACTORY_ADDRESS")!;

    var addressesToFund = (await Task.WhenAll(
        Enumerable
            .Range(1, 8)
            .Select(i => web3Faucet.Eth
                .GetContractQueryHandler<GetAddressMessage>()
                .QueryAsync<string>(
                    accountFactoryAddress,
                    new()
                    {
                        Owner = wallet.GetAccount(i).Address,
                        Salt = 0
                    }
                )
            )
    )).ToList();

    orchestrator = wallet.GetAccount(0);
    addressesToFund.Insert(0, orchestrator.Address);
    addressesToFund.Insert(0, "0x48e60BBb664aEfAc9f14aDB42e5FB5b4a119EB66"); // entrypoint

    foreach (var address in addressesToFund)
    {
        var txnReceipt = await web3Faucet.Eth
            .GetEtherTransferService()
            .TransferEtherAndWaitForReceiptAsync(
                toAddress: address,
                etherAmount: 1000.0m
            );

        Console.WriteLine($"Funded with Eth {address}. Txn hash: {txnReceipt.TransactionHash}");
    }
}
else
{
    orchestrator = new Account(Environment.GetEnvironmentVariable("ORCHESTRATOR_PRIVATE_KEY")!);
}

var web3 = new Web3(orchestrator, rpcUrl);

var contractNameToAddress = new Dictionary<string, string>();

{
    using var fsr = new FileStream("artifacts/Truthserum.bin", FileMode.Open, FileAccess.Read);
    using var sr = new StreamReader(fsr);

    TruthserumDeploymentMessage.Bytecode = await sr.ReadToEndAsync();

    var txnReceipt = await web3.Eth
        .GetContractDeploymentHandler<TruthserumDeploymentMessage>()
        .SendRequestAndWaitForReceiptAsync(new());

    Console.WriteLine($"Deployed Truthserum at {txnReceipt.ContractAddress}. Txn hash: {txnReceipt.TransactionHash}");
    contractNameToAddress["Truthserum"] = txnReceipt.ContractAddress;
}

{
    using var fsr = new FileStream("artifacts/RestrictedAccess.bin", FileMode.Open, FileAccess.Read);
    using var sr = new StreamReader(fsr);

    RestrictedAccessDeploymentMessage.Bytecode = await sr.ReadToEndAsync();

    var txnReceipt = await web3.Eth
        .GetContractDeploymentHandler<RestrictedAccessDeploymentMessage>()
        .SendRequestAndWaitForReceiptAsync(new());

    Console.WriteLine($"Deployed RestrictedAccess at {txnReceipt.ContractAddress}. Txn hash: {txnReceipt.TransactionHash}");
    contractNameToAddress["RestrictedAccess"] = txnReceipt.ContractAddress;
}

{
    using var fsr = new FileStream("artifacts/TruQuest.bin", FileMode.Open, FileAccess.Read);
    using var sr = new StreamReader(fsr);

    TruQuestDeploymentMessage.Bytecode = await sr.ReadToEndAsync();

    var txnReceipt = await web3.Eth
        .GetContractDeploymentHandler<TruQuestDeploymentMessage>()
        .SendRequestAndWaitForReceiptAsync(new()
        {
            TruthserumAddress = contractNameToAddress["Truthserum"],
            VerifierStake = 5 * 1000000,
            VerifierReward = 2 * 1000000,
            VerifierPenalty = 1 * 1000000,
            ThingStake = 25 * 1000000,
            ThingAcceptedReward = 7 * 1000000,
            ThingRejectedPenalty = 3 * 1000000,
            SettlementProposalStake = 25 * 1000000,
            SettlementProposalAcceptedReward = 7 * 1000000,
            SettlementProposalRejectedPenalty = 3 * 1000000
        });

    Console.WriteLine($"Deployed TruQuest at {txnReceipt.ContractAddress}. Txn hash: {txnReceipt.TransactionHash}");
    contractNameToAddress["TruQuest"] = txnReceipt.ContractAddress;
}

{
    using var fsr = new FileStream("artifacts/ThingValidationVerifierLottery.bin", FileMode.Open, FileAccess.Read);
    using var sr = new StreamReader(fsr);

    ThingValidationVerifierLotteryDeploymentMessage.Bytecode = await sr.ReadToEndAsync();

    var txnReceipt = await web3.Eth
        .GetContractDeploymentHandler<ThingValidationVerifierLotteryDeploymentMessage>()
        .SendRequestAndWaitForReceiptAsync(new()
        {
            TruQuestAddress = contractNameToAddress["TruQuest"],
            NumVerifiers = 3,
            DurationBlocks = 70
        });

    Console.WriteLine($"Deployed ThingValidationVerifierLottery at {txnReceipt.ContractAddress}. Txn hash: {txnReceipt.TransactionHash}");
    contractNameToAddress["ThingValidationVerifierLottery"] = txnReceipt.ContractAddress;
}

{
    using var fsr = new FileStream("artifacts/ThingValidationPoll.bin", FileMode.Open, FileAccess.Read);
    using var sr = new StreamReader(fsr);

    ThingValidationPollDeploymentMessage.Bytecode = await sr.ReadToEndAsync();

    var txnReceipt = await web3.Eth
        .GetContractDeploymentHandler<ThingValidationPollDeploymentMessage>()
        .SendRequestAndWaitForReceiptAsync(new()
        {
            TruQuestAddress = contractNameToAddress["TruQuest"],
            DurationBlocks = 30,
            VotingVolumeThresholdPercent = 50,
            MajorityThresholdPercent = 51
        });

    Console.WriteLine($"Deployed ThingValidationPoll at {txnReceipt.ContractAddress}. Txn hash: {txnReceipt.TransactionHash}");
    contractNameToAddress["ThingValidationPoll"] = txnReceipt.ContractAddress;
}

{
    using var fsr = new FileStream("artifacts/SettlementProposalAssessmentVerifierLottery.bin", FileMode.Open, FileAccess.Read);
    using var sr = new StreamReader(fsr);

    SettlementProposalAssessmentVerifierLotteryDeploymentMessage.Bytecode = await sr.ReadToEndAsync();

    var txnReceipt = await web3.Eth
        .GetContractDeploymentHandler<SettlementProposalAssessmentVerifierLotteryDeploymentMessage>()
        .SendRequestAndWaitForReceiptAsync(new()
        {
            TruQuestAddress = contractNameToAddress["TruQuest"],
            NumVerifiers = 3,
            DurationBlocks = 70
        });

    Console.WriteLine($"Deployed SettlementProposalAssessmentVerifierLottery at {txnReceipt.ContractAddress}. Txn hash: {txnReceipt.TransactionHash}");
    contractNameToAddress["SettlementProposalAssessmentVerifierLottery"] = txnReceipt.ContractAddress;
}

{
    using var fsr = new FileStream("artifacts/SettlementProposalAssessmentPoll.bin", FileMode.Open, FileAccess.Read);
    using var sr = new StreamReader(fsr);

    SettlementProposalAssessmentPollDeploymentMessage.Bytecode = await sr.ReadToEndAsync();

    var txnReceipt = await web3.Eth
        .GetContractDeploymentHandler<SettlementProposalAssessmentPollDeploymentMessage>()
        .SendRequestAndWaitForReceiptAsync(new()
        {
            TruQuestAddress = contractNameToAddress["TruQuest"],
            DurationBlocks = 30,
            VotingVolumeThresholdPercent = 50,
            MajorityThresholdPercent = 51
        });

    Console.WriteLine($"Deployed SettlementProposalAssessmentPoll at {txnReceipt.ContractAddress}. Txn hash: {txnReceipt.TransactionHash}");
    contractNameToAddress["SettlementProposalAssessmentPoll"] = txnReceipt.ContractAddress;
}

await web3.Eth
    .GetContractTransactionHandler<SetTruQuestAddressMessage>()
    .SendRequestAndWaitForReceiptAsync(
        contractNameToAddress["Truthserum"],
        new() { TruQuestAddress = contractNameToAddress["TruQuest"] }
    );

await web3.Eth
    .GetContractTransactionHandler<SetRestrictedAccessMessage>()
    .SendRequestAndWaitForReceiptAsync(
        contractNameToAddress["TruQuest"],
        new() { RestrictedAccessAddress = contractNameToAddress["RestrictedAccess"] }
    );

await web3.Eth
    .GetContractTransactionHandler<SetLotteryAndPollAddressesMessage>()
    .SendRequestAndWaitForReceiptAsync(
        contractNameToAddress["TruQuest"],
        new()
        {
            ThingValidationVerifierLotteryAddress = contractNameToAddress["ThingValidationVerifierLottery"],
            ThingValidationPollAddress = contractNameToAddress["ThingValidationPoll"],
            SettlementProposalAssessmentVerifierLotteryAddress = contractNameToAddress["SettlementProposalAssessmentVerifierLottery"],
            SettlementProposalAssessmentPollAddress = contractNameToAddress["SettlementProposalAssessmentPoll"]
        }
    );

await web3.Eth
    .GetContractTransactionHandler<EnableWithdrawalsMessage>()
    .SendRequestAndWaitForReceiptAsync(
        contractNameToAddress["TruQuest"],
        new() { Value = true }
    );

await web3.Eth
    .GetContractTransactionHandler<SetThingValidationPollMessage>()
    .SendRequestAndWaitForReceiptAsync(
        contractNameToAddress["ThingValidationVerifierLottery"],
        new() { ThingValidationPollAddress = contractNameToAddress["ThingValidationPoll"] }
    );

await web3.Eth
    .GetContractTransactionHandler<SetLotteryAddressesMessage>()
    .SendRequestAndWaitForReceiptAsync(
        contractNameToAddress["ThingValidationPoll"],
        new()
        {
            ThingValidationVerifierLotteryAddress = contractNameToAddress["ThingValidationVerifierLottery"],
            SettlementProposalAssessmentVerifierLotteryAddress = contractNameToAddress["SettlementProposalAssessmentVerifierLottery"]
        }
    );

await web3.Eth
    .GetContractTransactionHandler<SetPollsMessage>()
    .SendRequestAndWaitForReceiptAsync(
        contractNameToAddress["SettlementProposalAssessmentVerifierLottery"],
        new()
        {
            ThingValidationPollAddress = contractNameToAddress["ThingValidationPoll"],
            SettlementProposalAssessmentPollAddress = contractNameToAddress["SettlementProposalAssessmentPoll"]
        }
    );

await web3.Eth
    .GetContractTransactionHandler<SetSettlementProposalAssessmentVerifierLotteryAddressMessage>()
    .SendRequestAndWaitForReceiptAsync(
        contractNameToAddress["SettlementProposalAssessmentPoll"],
        new() { SettlementProposalAssessmentVerifierLotteryAddress = contractNameToAddress["SettlementProposalAssessmentVerifierLottery"] }
    );

if (environment == "Development")
{
    var wallet = new Wallet(Environment.GetEnvironmentVariable("MNEMONIC")!, seedPassword: null);
    var accountFactoryAddress = Environment.GetEnvironmentVariable("ACCOUNT_FACTORY_ADDRESS")!;

    var addressesToWhitelistAndFund = (await Task.WhenAll(
        Enumerable
            .Range(1, 8)
            .Select(i => web3.Eth
                .GetContractQueryHandler<GetAddressMessage>()
                .QueryAsync<string>(
                    accountFactoryAddress,
                    new()
                    {
                        Owner = wallet.GetAccount(i).Address,
                        Salt = 0
                    }
                )
            )
    )).ToList();

    await web3.Eth
        .GetContractTransactionHandler<GiveAccessToManyMessage>()
        .SendRequestAndWaitForReceiptAsync(contractNameToAddress["RestrictedAccess"], new()
        {
            Users = addressesToWhitelistAndFund
        });

    var txnDispatcher = web3.Eth.GetContractTransactionHandler<MintAndDepositTruthserumToMessage>();
    foreach (var address in addressesToWhitelistAndFund)
    {
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            contractNameToAddress["TruQuest"],
            new()
            {
                User = address,
                Amount = BigInteger.Parse("1000000000") // 1 TRU
            }
        );
    }

    Console.WriteLine($"Funded with Tru and gave restricted access to:\n\t{string.Join("\n\t", addressesToWhitelistAndFund)}");
}

using var fsw = new FileStream("artifacts/contract_addresses.json", FileMode.Create, FileAccess.Write);
await JsonSerializer.SerializeAsync<Dictionary<string, string>>(fsw, contractNameToAddress, new JsonSerializerOptions
{
    WriteIndented = true
});
await fsw.FlushAsync();
