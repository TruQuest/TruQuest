using System.Numerics;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Nethereum.Hex.HexTypes;

using Application.Ethereum.Models.IM;

namespace Infrastructure.Ethereum.ERC4337;

internal class BundlerApi
{
    private readonly ILogger<BundlerApi> _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly string _entryPointAddress;

    public BundlerApi(
        ILogger<BundlerApi> logger,
        IConfiguration configuration,
        IHttpClientFactory clientFactory
    )
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var network = configuration["Ethereum:Network"]!;
        _entryPointAddress = configuration[$"Ethereum:Contracts:{network}:EntryPoint:Address"]!;
    }

    public async Task<(BigInteger, BigInteger, BigInteger)?> EstimateUserOperationGas(UserOperation userOp)
    {
        using var client = _clientFactory.CreateClient("bundler");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/rpc");
        request.Content = new StringContent(
            JsonSerializer.Serialize(
                new RpcRequest
                {
                    Method = "eth_estimateUserOperationGas",
                    Params = new object[] { userOp, _entryPointAddress }
                },
                _serializerOptions
            ),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Error estimating user operation gas: {Reason}", response.ReasonPhrase);
            return null;
        }

        var doc = JsonSerializer.Deserialize<JsonDocument>(await response.Content.ReadAsStringAsync());
        if (doc!.RootElement.TryGetProperty("error", out JsonElement error))
        {
            _logger.LogWarning("Error estimating user operation gas: {Reason}", error.GetProperty("message").GetString());
            return null;
        }

        var gasEstimations = doc.RootElement.GetProperty("result");

        return (
            new HexBigInteger(gasEstimations.GetProperty("preVerificationGas").GetString()).Value,
            new HexBigInteger(gasEstimations.GetProperty("verificationGasLimit").GetString()).Value,
            new HexBigInteger(gasEstimations.GetProperty("callGasLimit").GetString()).Value
        );
    }

    public async Task<string?> SendUserOperation(UserOperation userOp)
    {
        using var client = _clientFactory.CreateClient("bundler");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/rpc");
        request.Content = new StringContent(
            JsonSerializer.Serialize(
                new RpcRequest
                {
                    Method = "eth_sendUserOperation",
                    Params = new object[] { userOp, _entryPointAddress }
                },
                _serializerOptions
            ),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Error sending user operation: {Reason}", response.ReasonPhrase);
            return null;
        }

        var doc = JsonSerializer.Deserialize<JsonDocument>(await response.Content.ReadAsStringAsync());
        if (doc!.RootElement.TryGetProperty("error", out JsonElement error))
        {
            _logger.LogWarning("Error sending user operation: {Reason}", error.GetProperty("message").GetString());
            return null;
        }

        return doc.RootElement.GetProperty("result").GetString()!;
    }

    public async Task<GetUserOperationReceiptVm?> GetUserOperationReceipt(string userOpHash)
    {
        using var client = _clientFactory.CreateClient("bundler");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/rpc");
        request.Content = new StringContent(
            JsonSerializer.Serialize(
                new RpcRequest
                {
                    Method = "eth_getUserOperationReceipt",
                    Params = new object[] { userOpHash }
                },
                _serializerOptions
            ),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Error retrieving user operation receipt: {Reason}", response.ReasonPhrase);
            return null;
        }

        var doc = JsonSerializer.Deserialize<JsonDocument>(await response.Content.ReadAsStringAsync());
        if (doc!.RootElement.TryGetProperty("error", out JsonElement error))
        {
            _logger.LogWarning("Error retrieving user operation receipt: {Reason}", error.GetProperty("message").GetString());
            return null;
        }

        var result = doc.RootElement.GetProperty("result");
        if (result.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return result.Deserialize<GetUserOperationReceiptVm>(_serializerOptions);
    }
}
