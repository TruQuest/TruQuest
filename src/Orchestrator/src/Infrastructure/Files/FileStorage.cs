using System.Text.Json;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Domain.Results;
using Domain.Errors;
using Application.Common.Interfaces;
using Application;

namespace Infrastructure.Files;

internal class FileStorage : IFileStorage
{
    private readonly ILogger<FileStorage> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public FileStorage(
        ILogger<FileStorage> logger,
        IHttpClientFactory clientFactory
    )
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task<Either<HandleError, string>> UploadJson(object obj)
    {
        using var span = Telemetry.StartActivity($"{GetType().FullName}.{nameof(UploadJson)}", kind: ActivityKind.Client);

        using var client = _clientFactory.CreateClient("ipfs");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v0/add?to-files=/");
        request.Content = new MultipartFormDataContent
        {
            {
                new StringContent(
                    JsonSerializer.Serialize(obj, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                ),
                "file",
                $"{Guid.NewGuid()}.json"
            }
        };

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(response.ReasonPhrase);
            return new HandleError(response.ReasonPhrase!);
        }

        var responseMap = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
            await response.Content.ReadAsStreamAsync()
        );

        return responseMap!["Hash"];
    }
}
