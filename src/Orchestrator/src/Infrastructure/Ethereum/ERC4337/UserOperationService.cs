using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nethereum.Contracts;
using Nethereum.Web3.Accounts;

using Application.Ethereum.Models.IM;

using Infrastructure.Ethereum.ERC4337;

public class UserOperationService
{
    private readonly ILogger<UserOperationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BundlerApi _bundlerApi;

    public UserOperationService(
        ILogger<UserOperationService> logger,
        IServiceProvider serviceProvider,
        BundlerApi bundlerApi
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _bundlerApi = bundlerApi;
    }

    public async Task<UserOperationError?> SendBatch(
        Account owner,
        List<(String TargetAddress, FunctionMessage Message)> actions,
        int confirmations = 1
    )
    {
        if (!actions.Any()) throw new ArgumentException("No actions specified");

        int attempts = 5;
        UserOperation userOp;
        UserOperationError? error;
        String? userOpHash = null;
        float preVerificationGasMultiplier = 1.1f,
            verificationGasLimitMultiplier = 1.5f,
            callGasLimitMultiplier = actions.Count * 3.0f;

        do
        {
            error = null;

            var userOpBuilder = _serviceProvider.GetRequiredService<UserOperationBuilder>()!;
            userOpBuilder = userOpBuilder
                .From(owner)
                .WithEstimatedGasLimitsMultipliers(
                    preVerificationGasMultiplier: preVerificationGasMultiplier,
                    verificationGasLimitMultiplier: verificationGasLimitMultiplier,
                    callGasLimitMultiplier: callGasLimitMultiplier
                );

            if (actions.Count == 1)
            {
                userOpBuilder = userOpBuilder.Action(actions.First().TargetAddress, actions.First().Message);
            }
            else
            {
                userOpBuilder = userOpBuilder.Actions(actions);
            }

            userOp = await userOpBuilder.Signed();

            _logger.LogInformation($"UserOp[{attempts}]:\n{userOp}");

            var result = await _bundlerApi.SendUserOperation(userOp);
            if (result.IsError)
            {
                error = result.Error!;
                _logger.LogWarning($"Error: [{error.Code}]: {error.Message}");
                if (error.IsPreVerificationGasTooLow)
                {
                    preVerificationGasMultiplier += 0.05f;
                }
                else if (error.IsOverVerificationGasLimit)
                {
                    verificationGasLimitMultiplier += 0.2f;
                }
                else
                {
                    break;
                }
            }
            else
            {
                userOpHash = result.Data!;
            }
        } while (userOpHash == null && --attempts > 0);

        Debug.Assert(userOpHash != null && error == null || userOpHash == null && error != null);

        if (error != null)
        {
            return error;
        }

        _logger.LogInformation($"UserOp Hash: {userOpHash}");

        GetUserOperationReceiptVm? receipt;
        do
        {
            await Task.Delay(2000);
            receipt = await _bundlerApi.GetUserOperationReceipt(userOpHash!);
        } while (receipt == null || receipt.Receipt.Confirmations < confirmations);

        _logger.LogInformation($"Receipt:\n{receipt}");

        if (!receipt.Success)
        {
            _logger.LogWarning($"UserOp Execution Failed. Reason: {receipt.Reason ?? "Unspecified"}");
            return new UserOperationError(receipt.Reason);
        }

        return null;
    }

    public Task<UserOperationError?> Send(
        Account owner, string targetAddress, FunctionMessage message, int confirmations = 1
    ) => SendBatch(
        owner: owner,
        actions: new() { (targetAddress, message) },
        confirmations: confirmations
    );
}
