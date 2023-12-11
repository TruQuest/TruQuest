using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

using Application.Common.Interfaces;

namespace Infrastructure.Email;

internal class EmailForwarder : IEmailForwarder
{
    private readonly ILogger<EmailForwarder> _logger;
    private readonly IAmazonS3 _amazonS3;
    private readonly IEmailSender _emailSender;

    private readonly string _path;

    public EmailForwarder(
        ILogger<EmailForwarder> logger,
        IAmazonS3 amazonS3,
        IEmailSender emailSender,
        IWebHostEnvironment hostEnvironment
    )
    {
        _logger = logger;
        _amazonS3 = amazonS3;
        _emailSender = emailSender;
        _path = hostEnvironment.ContentRootPath;
    }

    public async Task FetchAndForward()
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = "truquest-email",
            MaxKeys = 10,
        };

        var emailKeys = new List<string>();
        ListObjectsV2Response response;
        do
        {
            response = await _amazonS3.ListObjectsV2Async(request);
            emailKeys.AddRange(response.S3Objects.Select(obj => obj.Key));
            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated);

        if (!emailKeys.Any()) return;

        _logger.LogInformation($"There are {emailKeys.Count} emails in the bucket");

        var transferUtil = new TransferUtility(_amazonS3); // @@??: Can this be injected instead?
        var downloadPath = Path.Combine(_path, "truquest-email");

        _logger.LogInformation($"Downloading the contents of the bucket to {downloadPath}");

        int fileCountBefore = 0;
        if (Directory.Exists(downloadPath))
        {
            var files = Directory.GetFiles(downloadPath);
            fileCountBefore = files.Length;
        }

        await transferUtil.DownloadDirectoryAsync(new TransferUtilityDownloadDirectoryRequest
        {
            BucketName = "truquest-email",
            LocalDirectory = downloadPath,
            S3Directory = "/",
            DisableSlashCorrection = true
        });

        if (Directory.Exists(downloadPath))
        {
            var files = Directory.GetFiles(downloadPath);
            if (files.Length > fileCountBefore)
            {
                _logger.LogInformation($"Successfully downloaded {files.Length - fileCountBefore} email(s)");

                foreach (var file in files)
                {
                    await _emailSender.ForwardEmail("tru9quest@gmail.com", file);
                    _logger.LogInformation($"Successfully forwarded email: {Path.GetFileName(file)}");
                }

                var deleteResponse = await _amazonS3.DeleteObjectsAsync(new DeleteObjectsRequest
                {
                    BucketName = "truquest-email",
                    Objects = emailKeys.Select(key => new KeyVersion { Key = key }).ToList()
                });

                if (deleteResponse.DeleteErrors.Any())
                {
                    foreach (var error in deleteResponse.DeleteErrors)
                    {
                        _logger.LogError($"Error deleting email {error.Key} from the bucket: [{error.Code}] {error.Message}");
                    }
                }
                else
                {
                    _logger.LogInformation("Successfully deleted emails");
                }
            }
        }
    }
}
