namespace Services;

internal interface IFileStorage
{
    Task<(List<string> FileIpfsCids, string FolderIpfsCid)> Upload(string folderName, IEnumerable<string> filePaths);
    Task Delete(string folderIpfsCid);
}
