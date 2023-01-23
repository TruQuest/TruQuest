using System.Text.Json;

using Microsoft.Extensions.Logging;

using Domain.Results;
using Application.Common.Interfaces;
using Application.Common.Errors;

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

    public async Task<Either<FileError, string>> Upload(string filePath)
    {
        using var client = _clientFactory.CreateClient("ipfs");

        using var file = File.OpenRead(filePath);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v0/add?to-files=/");
        using var content = new MultipartFormDataContent {
            { new StreamContent(file), "file", Path.GetFileName(filePath) }
        };
        request.Content = content;

        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var responseMap = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
                await response.Content.ReadAsStreamAsync()
            );

            return responseMap!["Hash"];
        }

        _logger.LogWarning(response.ReasonPhrase);

        return new FileError(response.ReasonPhrase!);
    }

    public async Task<Either<FileError, string>> UploadJson(object obj)
    {
        using var client = _clientFactory.CreateClient("ipfs");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v0/add?to-files=/");
        using var content = new MultipartFormDataContent
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
        request.Content = content;

        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var responseMap = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
                await response.Content.ReadAsStreamAsync()
            );

            return responseMap!["Hash"];
        }

        _logger.LogWarning(response.ReasonPhrase);

        return new FileError(response.ReasonPhrase!);
    }
}