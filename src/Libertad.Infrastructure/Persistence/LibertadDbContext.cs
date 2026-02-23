using Libertad.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Libertad.Infrastructure.Persistence;

public class LibertadDbContext : DbContext
{
    public LibertadDbContext(DbContextOptions<LibertadDbContext> options)
        : base(options)
    {
    }

    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibertadDbContext).Assembly);
    }
}
