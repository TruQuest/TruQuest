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
        ImagePath
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
    private readonly IImageCropper _imageCropper;
    private readonly IWebPageScreenshotTaker _webPageScreenshotTaker;
    private readonly IFileStorage _fileStorage;

    private readonly string _path;

    public FileArchiver(
        ILogger<FileArchiver> logger,
        IConfiguration configuration,
        IImageSaver imageSaver,
        IImageCropper imageCropper,
        IWebPageScreenshotTaker webPageScreenshotTaker,
        IFileStorage fileStorage
    )
    {
        _logger = logger;
        _imageSaver = imageSaver;
        _imageCropper = imageCropper;
        _webPageScreenshotTaker = webPageScreenshotTaker;
        _fileStorage = fileStorage;

        _path = configuration["UserFiles:Path"]!;
    }

    public async Task<Error?> ArchiveAllAttachments(string requestId, object input, IProgress<int>? progress = null)
    {
        var error = await _archiveAllAttachments(requestId, input, progress);
        try
        {
            var dir = new DirectoryInfo($"{_path}/{requestId}");
            if (dir.Exists) dir.Delete(recursive: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error trying to delete directory {_path}/{requestId}");
        }

        return error;
    }

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
                            ExtraBackingProp = null,
                            AttachmentType = AttachmentType.ImagePath,
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
            if (task.AttachmentType is AttachmentType.WebPageUrl)
            {
                task.ExtraBackingProp!.SetValue(task.Object, task.ExtraIpfsCid);
            }
        }
    }

    private async Task<Error?> _archiveAllAttachments(string requestId, object input, IProgress<int>? progress)
    {
        var archiveTasks = new List<ArchiveTask>();
        _collectArchiveTasks(input, archiveTasks);

        if (archiveTasks.Any())
        {
            // @@NOTE: In the future it will be possible for user to, for example, do not send
            // any images and only provide evidence urls, meaning, the orchestrator wouldn't have
            // any files to receive and, therefore, wouldn't create this directory.
            Directory.CreateDirectory($"{_path}/{requestId}");

            progress?.Report(20);

            var saveImageTasks = archiveTasks
                .Where(t => t.AttachmentType == AttachmentType.ImageUrl)
                .Select(t => _imageSaver.SaveLocalCopy(requestId, t.Uri))
                .ToList();

            List<string> filePaths;
            try
            {
                filePaths = (await Task.WhenAll(saveImageTasks)).ToList();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error trying to save images to local drive");
                return new Error("Error trying to save images to local drive");
            }

            filePaths.AddRange(archiveTasks
                .Where(t => t.AttachmentType == AttachmentType.ImagePath)
                .Select(t => t.Uri)
            );

            List<string> ipfsCids;
            string? imageFolderIpfsCid;
            if (filePaths.Any())
            {
                try
                {
                    (ipfsCids, imageFolderIpfsCid) = await _fileStorage.Upload($"{requestId}-images", filePaths);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error trying to add images to ipfs");
                    return new Error("Error trying to add images to ipfs");
                }
            }
            else
            {
                ipfsCids = new();
                imageFolderIpfsCid = null;
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
                List<string>? webPageScreenshotFilePaths = await _webPageScreenshotTaker.Take(requestId, webPagesToArchive);
                if (webPageScreenshotFilePaths == null)
                {
                    _logger.LogWarning("Error trying to take webpage screenshots");

                    if (imageFolderIpfsCid != null) await _fileStorage.Delete(imageFolderIpfsCid);

                    return new Error("Error trying to take webpage screenshots");
                }

                progress?.Report(75);

                var cropImageTasks = webPageScreenshotFilePaths
                    .Select(path => _imageCropper.Crop(requestId, path))
                    .ToList(); // @@??: Semaphore?

                string[] previewImageFilePaths;
                try
                {
                    previewImageFilePaths = await Task.WhenAll(cropImageTasks);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error trying to crop webpage screenshots");

                    if (imageFolderIpfsCid != null) await _fileStorage.Delete(imageFolderIpfsCid);

                    return new Error("Error trying to crop webpage screenshots");
                }

                Debug.Assert(webPageScreenshotFilePaths.Count == previewImageFilePaths.Length);

                progress?.Report(85);

                try
                {
                    (ipfsCids, _) = await _fileStorage.Upload(
                        $"{requestId}-screenshots", webPageScreenshotFilePaths.Concat(previewImageFilePaths)
                    );
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error trying to add webpage screenshots to ipfs");

                    if (imageFolderIpfsCid != null) await _fileStorage.Delete(imageFolderIpfsCid);

                    return new Error("Error trying to add webpage screenshots to ipfs");
                }

                var webPageScreenshotIpfsCids = ipfsCids.Take(ipfsCids.Count / 2).ToList();
                var previewImageIpfsCids = ipfsCids.TakeLast(ipfsCids.Count / 2).ToList();

                Debug.Assert(webPageScreenshotIpfsCids.Count == previewImageIpfsCids.Count);
                Debug.Assert(
                    webPageScreenshotIpfsCids.Count == archiveTasks.Where(t => t.AttachmentType == AttachmentType.WebPageUrl).Count()
                );

                i = 0;
                foreach (var archiveWebPageTask in archiveTasks.Where(t => t.AttachmentType == AttachmentType.WebPageUrl))
                {
                    archiveWebPageTask.IpfsCid = webPageScreenshotIpfsCids[i];
                    archiveWebPageTask.ExtraIpfsCid = previewImageIpfsCids[i++];
                }
            }

            _setBackingFields(archiveTasks);
        }

        return null;
    }
}
