using Domain.Results;

using Application.Common.Errors;

namespace Application.Common.Interfaces;

public interface IFileStorage
{
    Task<Either<FileError, string>> UploadJson(object obj);
}