using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface ITagQueryable
{
    Task<IEnumerable<TagQm>> GetAll();
}