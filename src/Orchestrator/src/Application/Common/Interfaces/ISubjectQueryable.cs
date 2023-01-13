using Application.Subject.Queries.GetSubject;

namespace Application.Common.Interfaces;

public interface ISubjectQueryable
{
    Task<SubjectQm?> GetById(Guid id);
}