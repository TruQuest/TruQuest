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

    public async Task ArchiveAll(object input)
    {
        foreach (var prop in input.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
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
                        await ArchiveAll(elem);
                    }
                }
                else
                {
                    await ArchiveAll(prop.GetValue(input)!);
                }
            }
            else
            {
                var attr = prop.GetCustomAttribute<FileUrlAttribute>();
                string url;
                if (attr != null && (url = (string)prop.GetValue(input)!) != string.Empty)
                {
                    var backingProp = prop.DeclaringType!
                        .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                        .Single(p => p.Name == attr.BackingField);

                    if (attr is ImageUrlAttribute)
                    {
                        var response = await _requestDispatcher.GetResult(new ArchiveImageCommand { Url = url });
                        if (response is ArchiveImageSuccessResult result)
                        {
                            backingProp.SetValue(input, result.IpfsCid);
                        }
                    }
                    else if (attr is WebPageUrlAttribute webAttr)
                    {
                        var response = await _requestDispatcher.GetResult(new ArchiveWebPageCommand { Url = url });
                        if (response is ArchiveWebPageSuccessResult result)
                        {
                            backingProp.SetValue(input, result.HtmlIpfsCid);
                            prop.DeclaringType
                                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                                .Single(p => p.Name == webAttr.ExtraBackingField)
                                .SetValue(input, result.JpgIpfsCid);
                        }
                    }
                }
            }
        }
    }
}