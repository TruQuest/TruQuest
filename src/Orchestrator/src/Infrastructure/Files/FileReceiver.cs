using System.Buffers;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

using Domain.Errors;
using Domain.Results;
using Application;
using Application.Common.Interfaces;

namespace Infrastructure.Files;

internal class FileReceiver : IFileReceiver
{
    private readonly MultipartRequestHelper _multipartRequestHelper;
    private readonly ImageFileValidator _imageFileValidator;
    private readonly string _path;

    public FileReceiver(
        IConfiguration configuration,
        MultipartRequestHelper multipartRequestHelper,
        ImageFileValidator imageFileValidator
    )
    {
        _multipartRequestHelper = multipartRequestHelper;
        _imageFileValidator = imageFileValidator;
        _path = configuration["UserFiles:Path"]!;
    }

    public async Task<Either<HandleError, FormCollection>> ReceiveFilesAndFormValues(
        HttpRequest request, int maxSize, string filePrefix
    )
    {
        using var span = Telemetry.StartActivity($"{GetType().FullName}.{nameof(ReceiveFilesAndFormValues)}");

        if (!_multipartRequestHelper.IsMultipartContentType(request.ContentType))
        {
            return new HandleError("multipart/form-data request expected");
        }

        var filePaths = new List<string>();
        var formAccumulator = new KeyValueAccumulator();

        string boundary;
        try
        {
            boundary = _multipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(request.ContentType),
                FormOptions.DefaultMultipartBoundaryLengthLimit
            );
        }
        catch (InvalidDataException e)
        {
            return new HandleError(e.Message);
        }

        var reader = new MultipartReader(boundary, request.Body);

        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition, out var contentDisposition
            );

            if (hasContentDispositionHeader)
            {
                if (_multipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    var valid = false;
                    string? ext = null;
                    byte[]? header = null;

                    if (_imageFileValidator.IsFileDeclaredAsImage(contentDisposition!))
                    {
                        (valid, ext, header) = await _imageFileValidator.ValidateSignature(section, contentDisposition!);
                    }

                    if (!valid) return new HandleError("Invalid file format");

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var containerFilePath = $"/user_files/{filePrefix}/{fileName}";
                    var dir = $"{_path}/{filePrefix}";
                    Directory.CreateDirectory(dir);
                    var filePath = $"{dir}/{fileName}";

                    var maxSizeExceeded = false;
                    try
                    {
                        var fileDisposed = false;
                        var fs = new FileStream(filePath, FileMode.CreateNew);
                        int bufferSize = 64 * 1024; // @@TODO: Config.
                        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                        try
                        {
                            await fs.WriteAsync(new ReadOnlyMemory<byte>(header));
                            while (true)
                            {
                                int n = await section.Body.ReadAsync(new Memory<byte>(buffer));
                                if (n == 0) break;

                                maxSize -= n;
                                if (maxSize < 0)
                                {
                                    maxSizeExceeded = true;
                                    break;
                                }

                                await fs.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, n));
                            }
                        }
                        catch (Exception ex)
                        {
                            fs.Dispose();
                            fileDisposed = true;
                            File.Delete(filePath);

                            return new HandleError(ex.Message);
                        }
                        finally
                        {
                            if (!fileDisposed) fs.Dispose();
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }
                    // catch (IOException)
                    // {
                    //     return new HandleError("File uploading already in progress");
                    // }
                    catch (Exception ex)
                    {
                        return new HandleError(ex.Message);
                    }

                    if (maxSizeExceeded)
                    {
                        File.Delete(filePath);
                        return new HandleError("Max file size limit exceeded");
                    }

                    filePaths.Add(containerFilePath);
                }
                else if (_multipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                {
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition!.Name).Value!;
                    var encoding = _getEncoding(section);

                    using var streamReader = new StreamReader(
                        section.Body,
                        encoding,
                        detectEncodingFromByteOrderMarks: true,
                        bufferSize: 1024,
                        leaveOpen: true
                    );
                    // @@NOTE: The value length limit is enforced by MultipartBodyLengthLimit.
                    var value = await streamReader.ReadToEndAsync();
                    if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                    {
                        value = string.Empty;
                    }

                    formAccumulator.Append(key, value);
                }
            }

            section = await reader.ReadNextSectionAsync();
        }

        for (int i = 0; i < filePaths.Count; ++i) formAccumulator.Append($"file{i + 1}", filePaths[i]);

        return new FormCollection(formAccumulator.GetResults());
    }

    private Encoding _getEncoding(MultipartSection section)
    {
        MediaTypeHeaderValue.TryParse(section.ContentType, out var contentType);
        // @@NOTE: UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in most cases.
#pragma warning disable SYSLIB0001
        if (contentType?.Encoding == null || Encoding.UTF7.Equals(contentType.Encoding))
        {
            return Encoding.UTF8;
        }

        return contentType.Encoding;
    }
}
