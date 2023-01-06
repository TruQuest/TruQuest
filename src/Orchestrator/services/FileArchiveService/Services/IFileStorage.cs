namespace Services;

internal interface IFileStorage
{
    Task<string> Upload(string filePath);
    Task Delete(IEnumerable<string> ipfsCids);
}