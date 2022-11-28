using System.Reflection;
using System.Collections;

using Microsoft.Extensions.Logging;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Infrastructure.Files;

internal class FileFetcher : IFileFetcher
{
    private readonly ILogger<FileFetcher> _logger;
    private readonly IImageFetcher _imageFetcher;
    private readonly IWebPageScreenshotTaker _webPageScreenshotTaker;
    private readonly Assembly _inputModelsAssembly;

    public FileFetcher(
        ILogger<FileFetcher> logger,
        IImageFetcher imageFetcher,
        IWebPageScreenshotTaker webPageScreenshotTaker
    )
    {
        _logger = logger;
        _imageFetcher = imageFetcher;
        _webPageScreenshotTaker = webPageScreenshotTaker;
        _inputModelsAssembly = Assembly.GetAssembly(typeof(IFileFetcher))!;
    }

    public async IAsyncEnumerable<(string filePath, object obj, PropertyInfo prop)> FetchAll(object input, string userId)
    {
        foreach (var prop in input.GetType().GetProperties())
        {
            var propType = prop.PropertyType;
            Type? elemType = null;
            if (propType.IsAssignableTo(typeof(IEnumerable)) && propType != typeof(string))
            {
                propType = elemType = propType.GetGenericArguments().First();
            }

            if (propType.Assembly == _inputModelsAssembly)
            {
                if (elemType != null)
                {
                    foreach (var elem in (IEnumerable)prop.GetValue(input)!)
                    {
                        await foreach (var (filePath, obj, nestedProp) in FetchAll(elem, userId))
                        {
                            yield return (filePath, obj, nestedProp);
                        }
                    }
                }
                else
                {
                    await foreach (var (filePath, obj, nestedProp) in FetchAll(prop.GetValue(input)!, userId))
                    {
                        yield return (filePath, obj, nestedProp);
                    }
                }
            }
            else
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

                    yield return (filePath, input, prop);
                }
            }
        }
    }
}