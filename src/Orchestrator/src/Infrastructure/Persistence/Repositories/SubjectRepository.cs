using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class SubjectRepository : Repository<Subject>, ISubjectRepository
{
    private readonly AppDbContext _dbContext;

    public SubjectRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
    }

    public void Create(Subject subject)
    {
        _dbContext.Subjects.Add(subject);
    }

    public async Task<Subject?> GetById(Guid subjectId)
    {
        var subject = await _dbContext.Subjects
            .AsNoTracking()
            .Include(s => s.Tags)
            .SingleOrDefaultAsync(s => s.Id == subjectId);

        return subject;
    }
}