using System.Reflection;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Infrastructure.Files;

internal class FileFetcher : IFileFetcher
{
    private readonly IWebPageScreenshotTaker _webPageScreenshotTaker;

    public FileFetcher(IWebPageScreenshotTaker webPageScreenshotTaker)
    {
        _webPageScreenshotTaker = webPageScreenshotTaker;
    }

    public async IAsyncEnumerable<string> FetchAll<T>(T input, string userId)
    {
        foreach (var prop in typeof(T).GetProperties())
        {
            var attr = prop.GetCustomAttribute<FileURLAttribute>();
            string? url = null;
            if (attr != null && (url = (string)prop.GetValue(input)!) != string.Empty)
            {
                string filePath;
                if (attr is ImageURLAttribute)
                {
                    filePath = "";
                }
                else
                {
                    filePath = $"files/{userId}/webpages/{Guid.NewGuid()}.png";
                    await _webPageScreenshotTaker.Take(url, filePath);
                }

                yield return filePath;
            }
        }
    }
}