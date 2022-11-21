using System.Buffers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Domain.Results;
using Application.Common.Errors;
using Application.Common.Interfaces;

namespace Infrastructure.Files;

internal class ImageFetcher : IImageFetcher
{
    private readonly ILogger<ImageFetcher> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IImageSignatureVerifier _imageSignatureVerifier;

    private readonly HashSet<string> _acceptableMimeTypes;
    private readonly int _maxSizeBytes;
    private readonly int _fetchBufferSizeBytes;

    public ImageFetcher(
        IConfiguration configuration,
        ILogger<ImageFetcher> logger,
        IHttpClientFactory httpClientFactory,
        IImageSignatureVerifier imageSignatureVerifier
    )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _imageSignatureVerifier = imageSignatureVerifier;

        _acceptableMimeTypes = new(configuration["Files:Images:AcceptableMimeTypes"]!.Split(','));
        _maxSizeBytes = configuration.GetValue<int>("Files:Images:MaxSizeKb") * 1024;
        _fetchBufferSizeBytes = configuration.GetValue<int>("Files:FetchBufferSizeBytes");
    }

    public async Task<Either<FileError, string>> Fetch(string url, string filePathWithoutExt)
    {
        using var client = _httpClientFactory.CreateClient("image");
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(response.ReasonPhrase);
            return new FileError(response.ReasonPhrase!);
        }

        string? contentType;
        if (
            (contentType = response.Content.Headers.ContentType?.MediaType?.ToLower()) != null &&
            _acceptableMimeTypes.Contains(contentType)
        )
        {
            var fileExt = contentType.Split('/').Last();
            var filePath = $"{filePathWithoutExt}.{fileExt}";

            long? contentLength = response.Content.Headers.ContentLength;
            if (contentLength > _maxSizeBytes)
            {
                return new FileError("Max size exceeded");
            }

            int maxBytesToRead = contentLength != null ? (int)contentLength.Value : _maxSizeBytes + 1;
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

                            if (!signatureVerified)
                            {
                                break;
                            }
                        }
                        else
                        {
                            n = await istream.ReadAsync(buffer, 0, bufferSize);
                            if (n == 0)
                            {
                                break;
                            }
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
                return new FileError("Invalid file signature");
            }

            if (contentLength != null)
            {
                if (bytesRead != contentLength)
                {
                    File.Delete(filePath);
                    return new FileError(
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
                    return new FileError("Max size exceeded");
                }
            }

            return filePath;
        }

        return new FileError("Unsupported MIME-type");
    }
}