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
        Image,
        WebPage
    }

    private class ArchiveTask
    {
        public AttachmentType AttachmentType { get; init; }
        public required string Url { get; init; }
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

    public Task<Error?> ArchiveAllAttachments(object input, IProgress<int> progress) =>
        _archiveAllAttachments(input, progress);

    private void _collectArchiveTasks(object input, List<ArchiveTask> archiveTasks)
    {
        foreach (var prop in input.GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttribute<BackingFieldAttribute>() == null)
            .OrderBy(p => p.Name)
        )
        {
            var propType = prop.PropertyType;
            Type? elemType = null;
            if (propType.IsAssignableTo(typeof(IEnumerable)) && propType != typeof(string))
            {
                propType = elemType = propType.GetGenericArguments().First();
            }

            if (propType.Assembly == Assembly.GetExecutingAssembly())
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
                var attr = prop.GetCustomAttribute<FileUrlAttribute>();
                string? url;
                if (attr != null && (url = (string?)prop.GetValue(input)) != null)
                {
                    if (attr is ImageUrlAttribute)
                    {
                        archiveTasks.Add(new()
                        {
                            AttachmentType = AttachmentType.Image,
                            Url = url
                        });
                    }
                    else if (attr is WebPageUrlAttribute webAttr)
                    {
                        archiveTasks.Add(new()
                        {
                            AttachmentType = AttachmentType.WebPage,
                            Url = url
                        });
                    }
                }
            }
        }
    }

    private void _setBackingFields(object input, List<ArchiveTask> archiveTasks)
    {
        foreach (var prop in input.GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttribute<BackingFieldAttribute>() == null)
            .OrderBy(p => p.Name)
        )
        {
            var propType = prop.PropertyType;
            Type? elemType = null;
            if (propType.IsAssignableTo(typeof(IEnumerable)) && propType != typeof(string))
            {
                propType = elemType = propType.GetGenericArguments().First();
            }

            if (propType.Assembly == Assembly.GetExecutingAssembly())
            {
                if (elemType != null)
                {
                    foreach (var elem in (IEnumerable)prop.GetValue(input)!)
                    {
                        _setBackingFields(elem, archiveTasks);
                    }
                }
                else
                {
                    _setBackingFields(prop.GetValue(input)!, archiveTasks);
                }
            }
            else
            {
                var attr = prop.GetCustomAttribute<FileUrlAttribute>();
                string? url;
                if (attr != null && (url = (string?)prop.GetValue(input)) != null)
                {
                    var backingProp = prop.DeclaringType!
                        .GetProperty(attr.BackingField)!;

                    if (attr is ImageUrlAttribute)
                    {
                        Debug.Assert(archiveTasks.First().AttachmentType == AttachmentType.Image);
                        backingProp.SetValue(input, archiveTasks.First().IpfsCid!);
                    }
                    else if (attr is WebPageUrlAttribute webAttr)
                    {
                        Debug.Assert(archiveTasks.First().AttachmentType == AttachmentType.WebPage);
                        backingProp.SetValue(input, archiveTasks.First().IpfsCid!);
                        prop.DeclaringType
                            .GetProperty(webAttr.ExtraBackingField)!
                            .SetValue(input, archiveTasks.First().ExtraIpfsCid!);
                    }

                    archiveTasks.RemoveAt(0);
                }
            }
        }
    }

    private async Task<Error?> _archiveAllAttachments(object input, IProgress<int> progress)
    {
        var archiveTasks = new List<ArchiveTask>();
        _collectArchiveTasks(input, archiveTasks);

        if (archiveTasks.Any())
        {
            progress.Report(20);

            var saveImageTasks = archiveTasks
                .Where(t => t.AttachmentType == AttachmentType.Image)
                .Select(t => _imageSaver.SaveLocalCopy(t.Url))
                .ToList();

            string[] filePaths;
            try
            {
                filePaths = await Task.WhenAll(saveImageTasks);
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

            // @@TODO: Upload directory instead of separate files.
            var uploadImageTasks = filePaths
                .Select(filePath => _fileStorage.Upload(filePath))
                .ToList();

            string[] ipfsCids;
            try
            {
                ipfsCids = await Task.WhenAll(uploadImageTasks);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error adding images to ipfs");
                foreach (var task in saveImageTasks)
                {
                    var filePath = await task;
                    File.Delete(filePath);
                }

                var ipfsCidsToCleanup = new List<string>();
                foreach (var task in uploadImageTasks)
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        ipfsCidsToCleanup.Add(await task);
                    }
                }
                if (ipfsCidsToCleanup.Any())
                {
                    await _fileStorage.Delete(ipfsCidsToCleanup);
                }

                return new Error("Error adding images to ipfs");
            }

            progress.Report(40);

            Debug.Assert(
                ipfsCids.Length == archiveTasks.Where(t => t.AttachmentType == AttachmentType.Image).Count()
            );

            int i = 0;
            foreach (var archiveImageTask in archiveTasks.Where(t => t.AttachmentType == AttachmentType.Image))
            {
                archiveImageTask.IpfsCid = ipfsCids[i++];
            }

            var webPagesToArchive = archiveTasks
                .Where(t => t.AttachmentType == AttachmentType.WebPage)
                .Select(t => t.Url)
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

                progress.Report(75);

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

                progress.Report(85);

                var uploadWebPageTasks = htmlFilePaths
                    .Concat(previewImageFilePaths)
                    .Select(filePath => _fileStorage.Upload(filePath))
                    .ToList();

                try
                {
                    ipfsCids = await Task.WhenAll(uploadWebPageTasks);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error adding webpages to ipfs");

                    var outputFilesDir = Path.GetDirectoryName(htmlFilePaths.First())!;
                    Directory.Delete(outputFilesDir, recursive: true);

                    var ipfsCidsToCleanup = new List<string>();
                    foreach (var task in uploadWebPageTasks)
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            ipfsCidsToCleanup.Add(await task);
                        }
                    }
                    if (ipfsCidsToCleanup.Any())
                    {
                        await _fileStorage.Delete(ipfsCidsToCleanup);
                    }

                    return new Error("Error adding webpages to ipfs");
                }

                var htmlIpfsCids = ipfsCids.Take(ipfsCids.Length / 2).ToList();
                var previewImageIpfsCids = ipfsCids.TakeLast(ipfsCids.Length / 2).ToList();

                Debug.Assert(htmlIpfsCids.Count == previewImageIpfsCids.Count);
                Debug.Assert(
                    htmlIpfsCids.Count == archiveTasks.Where(t => t.AttachmentType == AttachmentType.WebPage).Count()
                );

                i = 0;
                foreach (var archiveWebPageTask in archiveTasks.Where(t => t.AttachmentType == AttachmentType.WebPage))
                {
                    archiveWebPageTask.IpfsCid = htmlIpfsCids[i];
                    archiveWebPageTask.ExtraIpfsCid = previewImageIpfsCids[i++];
                }
            }

            _setBackingFields(input, archiveTasks);
            Debug.Assert(!archiveTasks.Any());
        }

        return null;
    }
}