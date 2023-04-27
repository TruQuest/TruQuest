using System.Collections;
using System.Diagnostics;
using System.Reflection;

using Attributes;
using Utils;

namespace Services;

internal class FileArchiver : IFileArchiver
{
    private enum AttachmentType
    {
        ImageUrl,
        WebPageUrl,
        ImagePath,
        WebPagePath
    }

    private class ArchiveTask
    {
        public required object Object { get; init; }
        public required PropertyInfo BackingProp { get; init; }
        public required PropertyInfo? ExtraBackingProp { get; init; }
        public required AttachmentType AttachmentType { get; init; }
        public required string Uri { get; init; }
        public string? IpfsCid { get; set; }
        public string? ExtraIpfsCid { get; set; }
    }

    private readonly ILogger<FileArchiver> _logger;
    private readonly IImageSaver _imageSaver;
    private readonly IWebPageSaver _webPageSaver;
    private readonly IWebPageScreenshotTaker _webPageScreenshotTaker;
    private readonly IFileStorage _fileStorage;

    public FileArchiver(
        ILogger<FileArchiver> logger,
        IImageSaver imageSaver,
        IWebPageSaver webPageSaver,
        IWebPageScreenshotTaker webPageScreenshotTaker,
        IFileStorage fileStorage
    )
    {
        _logger = logger;
        _imageSaver = imageSaver;
        _webPageSaver = webPageSaver;
        _webPageScreenshotTaker = webPageScreenshotTaker;
        _fileStorage = fileStorage;
    }

    public Task<Error?> ArchiveAllAttachments(object input, IProgress<int>? progress = null) =>
        _archiveAllAttachments(input, progress);

    private void _collectArchiveTasks(object input, List<ArchiveTask> archiveTasks)
    {
        foreach (var prop in input.GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttribute<BackingFieldAttribute>() == null)
        )
        {
            var propType = prop.PropertyType;
            Type? elemType = null;
            // @@TODO: Check against IEnumerable<T>.
            if (propType.IsAssignableTo(typeof(IEnumerable)) && propType != typeof(string))
            {
                propType = elemType = propType.GetGenericArguments().First();
            }

            if (propType.Assembly == Assembly.GetExecutingAssembly() && !propType.IsEnum)
            {
                if (elemType != null)
                {
                    foreach (var elem in (IEnumerable)prop.GetValue(input)!)
                    {
                        _collectArchiveTasks(elem, archiveTasks);
                    }
                }
                else
                {
                    _collectArchiveTasks(prop.GetValue(input)!, archiveTasks);
                }
            }
            else
            {
                var urlAttr = prop.GetCustomAttribute<FileUrlAttribute>();
                string? url;
                if (urlAttr != null && (url = (string?)prop.GetValue(input)) != null)
                {
                    archiveTasks.Add(new()
                    {
                        Object = input,
                        BackingProp = prop.DeclaringType!.GetProperty(urlAttr.BackingField)!,
                        ExtraBackingProp = urlAttr is WebPageUrlAttribute webAttr ?
                            prop.DeclaringType.GetProperty(webAttr.ExtraBackingField)! :
                            null,
                        AttachmentType = urlAttr is ImageUrlAttribute ?
                            AttachmentType.ImageUrl :
                            AttachmentType.WebPageUrl,
                        Uri = url
                    });
                }
                else
                {
                    var pathAttr = prop.GetCustomAttribute<FilePathAttribute>();
                    string? path;
                    if (pathAttr != null && (path = (string?)prop.GetValue(input)) != null)
                    {
                        archiveTasks.Add(new()
                        {
                            Object = input,
                            BackingProp = prop.DeclaringType!.GetProperty(pathAttr.BackingField)!,
                            ExtraBackingProp = pathAttr is WebPagePathAttribute webAttr ?
                                prop.DeclaringType.GetProperty(webAttr.ExtraBackingField)! :
                                null,
                            AttachmentType = pathAttr is ImagePathAttribute ?
                                AttachmentType.ImagePath :
                                AttachmentType.WebPagePath,
                            Uri = path
                        });
                    }
                }
            }
        }
    }

    private void _setBackingFields(List<ArchiveTask> archiveTasks)
    {
        foreach (var task in archiveTasks)
        {
            task.BackingProp.SetValue(task.Object, task.IpfsCid);
            if (task.AttachmentType is AttachmentType.WebPageUrl or AttachmentType.WebPagePath)
            {
                task.ExtraBackingProp!.SetValue(task.Object, task.ExtraIpfsCid);
            }
        }
    }

    private async Task<Error?> _archiveAllAttachments(object input, IProgress<int>? progress)
    {
        var archiveTasks = new List<ArchiveTask>();
        _collectArchiveTasks(input, archiveTasks);

        if (archiveTasks.Any())
        {
            progress?.Report(20);

            var saveImageTasks = archiveTasks
                .Where(t => t.AttachmentType == AttachmentType.ImageUrl)
                .Select(t => _imageSaver.SaveLocalCopy(t.Uri))
                .ToList();

            List<string> filePaths;
            try
            {
                filePaths = (await Task.WhenAll(saveImageTasks)).ToList();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error saving images to local drive");
                foreach (var task in saveImageTasks)
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        var filePath = await task;
                        File.Delete(filePath);
                    }
                }

                return new Error("Error saving images to local drive");
            }

            filePaths.AddRange(archiveTasks
                .Where(t => t.AttachmentType == AttachmentType.ImagePath)
                .Select(t => t.Uri)
            );

            // @@TODO: Check non-emptiness of filePaths.

            List<string> ipfsCids;
            try
            {
                ipfsCids = await _fileStorage.Upload(filePaths);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error adding images to ipfs");
                foreach (var task in saveImageTasks)
                {
                    var filePath = await task;
                    File.Delete(filePath);
                }

                return new Error("Error adding images to ipfs");
            }

            progress?.Report(40);

            Debug.Assert(
                ipfsCids.Count == archiveTasks
                    .Where(t => t.AttachmentType is AttachmentType.ImageUrl or AttachmentType.ImagePath)
                    .Count()
            );

            int i = 0;
            foreach (var archiveImageTask in archiveTasks.Where(t => t.AttachmentType == AttachmentType.ImageUrl))
            {
                archiveImageTask.IpfsCid = ipfsCids[i++];
            }
            foreach (var archiveImageTask in archiveTasks.Where(t => t.AttachmentType == AttachmentType.ImagePath))
            {
                archiveImageTask.IpfsCid = ipfsCids[i++];
            }

            var webPagesToArchive = archiveTasks
                .Where(t => t.AttachmentType == AttachmentType.WebPageUrl)
                .Select(t => t.Uri)
                .ToList();

            if (webPagesToArchive.Any())
            {
                List<string> htmlFilePaths;
                try
                {
                    htmlFilePaths = await _webPageSaver.SaveLocalCopies(webPagesToArchive);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error saving webpages to local drive");
                    return new Error("Error saving webpages to local drive");
                }

                progress?.Report(75);

                List<string> previewImageFilePaths;
                try
                {
                    previewImageFilePaths = await _webPageScreenshotTaker.Take(htmlFilePaths);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error taking webpage screenshots");
                    var outputFilesDir = Path.GetDirectoryName(htmlFilePaths.First())!;
                    Directory.Delete(outputFilesDir, recursive: true);

                    return new Error("Error taking webpage screenshots");
                }

                Debug.Assert(htmlFilePaths.Count == previewImageFilePaths.Count);

                progress?.Report(85);

                try
                {
                    ipfsCids = await _fileStorage.Upload(htmlFilePaths.Concat(previewImageFilePaths));
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error adding webpages to ipfs");

                    var outputFilesDir = Path.GetDirectoryName(htmlFilePaths.First())!;
                    Directory.Delete(outputFilesDir, recursive: true);

                    return new Error("Error adding webpages to ipfs");
                }

                var htmlIpfsCids = ipfsCids.Take(ipfsCids.Count / 2).ToList();
                var previewImageIpfsCids = ipfsCids.TakeLast(ipfsCids.Count / 2).ToList();

                Debug.Assert(htmlIpfsCids.Count == previewImageIpfsCids.Count);
                Debug.Assert(
                    htmlIpfsCids.Count == archiveTasks.Where(t => t.AttachmentType == AttachmentType.WebPageUrl).Count()
                );

                i = 0;
                foreach (var archiveWebPageTask in archiveTasks.Where(t => t.AttachmentType == AttachmentType.WebPageUrl))
                {
                    archiveWebPageTask.IpfsCid = htmlIpfsCids[i];
                    archiveWebPageTask.ExtraIpfsCid = previewImageIpfsCids[i++];
                }
            }

            _setBackingFields(archiveTasks);
        }

        return null;
    }
}