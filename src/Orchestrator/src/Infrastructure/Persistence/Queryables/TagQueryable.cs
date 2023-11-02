using Microsoft.EntityFrameworkCore;

using Application.Common.Interfaces;
using Application.Common.Models.QM;

namespace Infrastructure.Persistence.Queryables;

internal class TagQueryable : Queryable, ITagQueryable
{
    private new readonly AppDbContext _dbContext;

    public TagQueryable(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<TagQm>> GetAll()
    {
        var tags = await _dbContext.Tags
            .Select(t => new TagQm
            {
                Id = t.Id!.Value,
                Name = t.Name
            })
            .ToListAsync();

        return tags;
    }
}
