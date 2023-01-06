namespace Application.Common.Interfaces;

public interface IFileArchiver
{
    Task ArchiveAll(object input);
}