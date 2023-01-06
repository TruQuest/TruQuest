using System.Text.Json;

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

    public async Task<List<string>> Upload(IEnumerable<string> filePaths)
    {
        var folder = Guid.NewGuid();
        using var client = _httpClientFactory.CreateClient("ipfs");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v0/add?to-files=/");
        var content = new MultipartFormDataContent(); // @@NOTE: Gets disposed by the request and in turn disposes of inner http contents, which dispose of file streams.
        int i = 1;
        foreach (var filePath in filePaths)
        {
            var file = File.OpenRead(filePath);
            content.Add(new StreamContent(file), $"file{i++}", $"{folder}/{Path.GetFileName(filePath)}");
        }
        request.Content = content;

        // @@TODO: Dispose of the request earlier.

        var cids = new List<string>(filePaths.Count() + 1);
        try
        {
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var sr = new StreamReader(await response.Content.ReadAsStreamAsync());
            string? line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                var responseMap = JsonSerializer.Deserialize<Dictionary<string, string>>(line)!;
                cids.Add(responseMap["Hash"]);
            }
        }
        catch
        {
            if (cids.Any())
            {
                if (cids.Count > filePaths.Count())
                {
                    using var unpinRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v0/pin/rm?arg={cids.Last()}&recursive=true");
                    await client.SendAsync(unpinRequest);
                }
                else
                {
                    var tasks = new List<Task<HttpResponseMessage>>();
                    foreach (var cid in cids)
                    {
                        var unpinRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v0/pin/rm?arg={cid}&recursive=false");
                        tasks.Add(client.SendAsync(unpinRequest));
                    }

                    await Task.WhenAll(tasks);
                }
            }
            throw;
        }

        return cids.SkipLast(1).ToList();
    }

    public async Task Delete(IEnumerable<string> ipfsCids) { }
}