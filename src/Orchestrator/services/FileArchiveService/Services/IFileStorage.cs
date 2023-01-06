namespace Services;

internal interface IFileStorage
{
    Task<List<string>> Upload(IEnumerable<string> filePaths);
    Task Delete(IEnumerable<string> ipfsCids);
}