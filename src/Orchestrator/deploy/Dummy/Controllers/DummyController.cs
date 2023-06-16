using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

namespace Dummy.Controllers;

[Route("[controller]")]
public class DummyController : ControllerBase
{
    private readonly ILogger<DummyController> _logger;
    private readonly string _ipfsHost;

    public DummyController(ILogger<DummyController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _ipfsHost = configuration["IPFS:Host"]!;
    }

    [HttpGet("upload")]
    public async Task<IActionResult> Upload()
    {
        var obj = new
        {
            Name = "Win!"
        };

        using var client = new HttpClient();
        client.BaseAddress = new Uri(_ipfsHost);

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
        if (response.IsSuccessStatusCode)
        {
            var responseMap = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
                await response.Content.ReadAsStreamAsync()
            );

            return Ok(responseMap!["Hash"]);
        }

        _logger.LogWarning(response.ReasonPhrase);

        return new ObjectResult(response.ReasonPhrase!)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
    }
}
