using System.Buffers;
using System.Diagnostics;

using Common.Monitoring;

namespace Services;

internal class ImageSaver : IImageSaver
{
    private readonly ILogger<ImageSaver> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IImageSignatureVerifier _imageSignatureVerifier;

    private readonly string _path;
    private readonly HashSet<string> _acceptableMimeTypes;
    private readonly int _userImageMaxSizeBytes;
    private readonly int _webPageScreenshotMaxSizeBytes;
    private readonly int _fetchBufferSizeBytes;

    public ImageSaver(
        IConfiguration configuration,
        ILogger<ImageSaver> logger,
        IHttpClientFactory httpClientFactory,
        IImageSignatureVerifier imageSignatureVerifier
    )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _imageSignatureVerifier = imageSignatureVerifier;

        _path = configuration["UserFiles:Path"]!;
        _acceptableMimeTypes = new(configuration["Images:AcceptableMimeTypes"]!.Split(','));
        _userImageMaxSizeBytes = configuration.GetValue<int>("Images:MaxSizeKb") * 1024;
        _webPageScreenshotMaxSizeBytes = configuration.GetValue<int>("WebPageScreenshots:ApiFlash:MaxSizeKb") * 1024;
        _fetchBufferSizeBytes = configuration.GetValue<int>("Images:FetchBufferSizeBytes");
    }

    public async Task<string> SaveLocalCopy(string requestId, string url, bool isWebPageScreenshot = false)
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(SaveLocalCopy)}", kind: ActivityKind.Client
        );

        using var client = _httpClientFactory.CreateClient("image");
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new Exception(response.ReasonPhrase!);

        string? contentType;
        if (
            (contentType = response.Content.Headers.ContentType?.MediaType?.ToLower()) == null ||
            !_acceptableMimeTypes.Contains(contentType)
        )
        {
            throw new Exception("Unsupported MIME-type");
        }

        var fileExt = contentType.Split('/').Last();
        var filePath = $"{_path}/{requestId}/{Guid.NewGuid()}.{fileExt}";

        long? contentLength = response.Content.Headers.ContentLength;
        var maxSizeBytes = isWebPageScreenshot ? _webPageScreenshotMaxSizeBytes : _userImageMaxSizeBytes;
        if (contentLength > maxSizeBytes) throw new Exception("Max size exceeded");

        int maxBytesToRead = contentLength != null ? (int)contentLength.Value : maxSizeBytes + 1;
        int bufferSize = Math.Min(_fetchBufferSizeBytes, maxBytesToRead);
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        int bytesRead = 0;
        int signatureSize = _imageSignatureVerifier.GetSignatureSize(fileExt);
        var signatureVerified = false;

        using (var fstream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
        {
            using (var istream = await response.Content.ReadAsStreamAsync())
            {
                while (bytesRead < maxBytesToRead)
                {
                    int n;
                    if (!signatureVerified)
                    {
                        n = await istream.ReadAtLeastAsync(buffer, signatureSize, throwOnEndOfStream: false);
                        if (n >= signatureSize)
                        {
                            signatureVerified = _imageSignatureVerifier.Verify(buffer, fileExt);
                        }

                        if (!signatureVerified) break;
                    }
                    else
                    {
                        n = await istream.ReadAsync(buffer, 0, bufferSize);
                        if (n == 0) break;
                    }

                    await fstream.WriteAsync(buffer, 0, n);
                    bytesRead += n;
                }
            }

            await fstream.FlushAsync();
        }

        ArrayPool<byte>.Shared.Return(buffer);

        if (!signatureVerified)
        {
            File.Delete(filePath);
            throw new Exception("Invalid file signature");
        }

        if (contentLength != null)
        {
            if (bytesRead != contentLength)
            {
                File.Delete(filePath);
                throw new Exception(
                    "Either connection to the image hosting site was interrupted or " +
                    "the image's declared size is less than its actual size"
                );
            }
        }
        else
        {
            if (bytesRead >= maxBytesToRead)
            {
                File.Delete(filePath);
                throw new Exception("Max size exceeded");
            }
        }

        return filePath;
    }
}
