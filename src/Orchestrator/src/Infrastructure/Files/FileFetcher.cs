using System.Reflection;

using Microsoft.Extensions.Logging;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Infrastructure.Files;

internal class FileFetcher : IFileFetcher
{
    private readonly ILogger<FileFetcher> _logger;
    private readonly IImageFetcher _imageFetcher;
    private readonly IWebPageScreenshotTaker _webPageScreenshotTaker;

    public FileFetcher(
        ILogger<FileFetcher> logger,
        IImageFetcher imageFetcher,
        IWebPageScreenshotTaker webPageScreenshotTaker
    )
    {
        _logger = logger;
        _imageFetcher = imageFetcher;
        _webPageScreenshotTaker = webPageScreenshotTaker;
    }

    public async IAsyncEnumerable<(string filePath, PropertyInfo prop)> FetchAll<T>(T input, string userId)
    {
        foreach (var prop in typeof(T).GetProperties())
        {
            var attr = prop.GetCustomAttribute<FileURLAttribute>();
            string url;
            if (attr != null && (url = (string)prop.GetValue(input)!) != string.Empty)
            {
                string filePath;
                if (attr is ImageURLAttribute)
                {
                    Directory.CreateDirectory($"files/{userId}/images");
                    filePath = $"files/{userId}/images/{Guid.NewGuid()}";
                    var result = await _imageFetcher.Fetch(url, filePath);
                    if (!result.IsError)
                    {
                        filePath = result.Data!;
                    }
                    else
                    {
                        _logger.LogWarning(result.Error!.ToString());
                    }
                }
                else
                {
                    filePath = $"files/{userId}/webpages/{Guid.NewGuid()}.png";
                    await _webPageScreenshotTaker.Take(url, filePath);
                }

                yield return (filePath, prop);
            }
        }
    }
}