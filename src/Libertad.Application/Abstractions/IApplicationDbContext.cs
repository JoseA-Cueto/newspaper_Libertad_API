using Libertad.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Libertad.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Section> Sections { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
