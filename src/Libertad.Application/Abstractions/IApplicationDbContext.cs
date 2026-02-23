using Libertad.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Libertad.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Section> Sections { get; }
    DbSet<Article> Articles { get; }
    DbSet<ReviewComment> ReviewComments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
