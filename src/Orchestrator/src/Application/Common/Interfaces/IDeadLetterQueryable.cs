namespace Application.Common.Interfaces;

public interface IDeadLetterQueryable
{
    Task<int> GetUnhandledCount();
}
