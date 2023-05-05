using Application.Subject.Queries.GetSubject;
using Application.Subject.Queries.GetSubjects;

namespace Application.Common.Interfaces;

public interface ISubjectQueryable
{
    Task<IEnumerable<SubjectPreviewQm>> GetAll();
    Task<SubjectQm?> GetById(Guid id);
}