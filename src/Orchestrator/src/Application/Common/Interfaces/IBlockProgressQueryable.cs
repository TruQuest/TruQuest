namespace Application.Common.Interfaces;

public interface IBlockProgressQueryable
{
    Task<long?> GetLastProcessedBlock();
}
