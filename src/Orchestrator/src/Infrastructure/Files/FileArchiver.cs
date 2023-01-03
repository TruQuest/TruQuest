using System.Reflection;
using System.Collections;

using Microsoft.Extensions.Logging;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Messages.Requests;
using Application.Common.Messages.Responses;

namespace Infrastructure.Files;

internal class FileArchiver : IFileArchiver
{
    private readonly ILogger<FileArchiver> _logger;
    private readonly IRequestDispatcher _requestDispatcher;
    private readonly Assembly _inputModelsAssembly;

    public FileArchiver(ILogger<FileArchiver> logger, IRequestDispatcher requestDispatcher)
    {
        _logger = logger;
        _requestDispatcher = requestDispatcher;
        _inputModelsAssembly = Assembly.GetAssembly(typeof(IFileArchiver))!;
    }

    public async IAsyncEnumerable<(string ipfsCid, object obj, PropertyInfo prop)> ArchiveAll(
        object input, string userId
    )
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
                        await foreach (var (ipfsCid, obj, nestedProp) in ArchiveAll(elem, userId))
                        {
                            yield return (ipfsCid, obj, nestedProp);
                        }
                    }
                }
                else
                {
                    await foreach (var (ipfsCid, obj, nestedProp) in ArchiveAll(prop.GetValue(input)!, userId))
                    {
                        yield return (ipfsCid, obj, nestedProp);
                    }
                }
            }
            else
            {
                var attr = prop.GetCustomAttribute<FileUrlAttribute>();
                string url;
                if (attr != null && (url = (string)prop.GetValue(input)!) != string.Empty)
                {
                    string ipfsCid;
                    if (attr is ImageUrlAttribute)
                    {
                        ipfsCid = "";
                        var response = await _requestDispatcher.Dispatch(new ArchiveImageCommand { Url = url });
                        if (response is ArchiveImageSuccessResult result)
                        {
                            ipfsCid = result.IpfsCid;
                        }
                    }
                    else
                    {
                        ipfsCid = "";
                        var response = await _requestDispatcher.Dispatch(new ArchiveWebPageCommand { Url = url });
                        if (response is ArchiveWebPageSuccessResult result)
                        {
                            ipfsCid = result.IpfsCid;
                        }
                    }

                    yield return (ipfsCid, input, prop);
                }
            }
        }
    }
}