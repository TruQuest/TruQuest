using System.Text.Json;

using Utils;

namespace Services;

internal class FileStorage : IFileStorage
{
    private readonly ILogger<FileStorage> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public FileStorage(
        ILogger<FileStorage> logger,
        IHttpClientFactory httpClientFactory
    )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Either<Error, string>> Upload(string filePath)
    {
        using var client = _httpClientFactory.CreateClient("ipfs");

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

        return new Error(response.ReasonPhrase!);
    }

    public async Task<Either<Error, string>> UploadJson(object obj)
    {
        using var client = _httpClientFactory.CreateClient("ipfs");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v0/add?to-files=/");
        using var content = new MultipartFormDataContent {{
            new StringContent(JsonSerializer.Serialize(obj, new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })),
            "file",
            $"{Guid.NewGuid()}.json"
        }};
        request.Content = content;

        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var responseMap = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
                await response.Content.ReadAsStreamAsync()
            );

            return responseMap!["Hash"];
        }

        return new Error(response.ReasonPhrase!);
    }
}