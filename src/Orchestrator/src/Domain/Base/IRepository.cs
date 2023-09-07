namespace Domain.Base;

public interface IRepository<T> where T : IAggregateRoot
{
    Task<int> SaveChanges();
}
