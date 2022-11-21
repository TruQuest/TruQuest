using Domain.Results;

using Application.Common.Errors;

namespace Application.Common.Interfaces;

public interface IImageFetcher
{
    Task<Either<FileError, string>> Fetch(string url, string filePathWithoutExt);
}