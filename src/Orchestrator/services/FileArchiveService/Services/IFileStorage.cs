using Utils;

namespace Services;

internal interface IFileStorage
{
    Task<Either<Error, string>> Upload(string filePath);
    Task<Either<Error, string>> UploadJson(object obj);
}