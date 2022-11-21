using Domain.Base;

namespace Domain.Aggregates;

public interface ISubjectRepository : IRepository<Subject>
{
    void Create(Subject subject);
    Task<Subject?> GetById(Guid subjectId);
}