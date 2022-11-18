namespace Domain.Base;

public interface IRepository<T> : IDisposable where T : IAggregateRoot
{
    ValueTask SaveChanges();
}