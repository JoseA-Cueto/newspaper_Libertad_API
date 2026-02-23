using Libertad.Application.Abstractions;
using Libertad.Application.Common;
using Libertad.Contracts.Sections;
using Libertad.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Libertad.Application.Sections.Services;

public class SectionService : ISectionService
{
    private readonly IApplicationDbContext _dbContext;

    public SectionService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SectionResponse>> GetActiveSectionsAsync(CancellationToken cancellationToken = default)
    {
        var sections = await _dbContext.Sections
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return sections.Select(MapToResponse).ToList();
    }

    public async Task<SectionResponse> CreateSectionAsync(CreateSectionRequest request, CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Name)
            : SlugHelper.GenerateSlug(request.Slug);

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new InvalidOperationException("Slug could not be generated.");
        }

        var slugExists = await _dbContext.Sections
            .AsNoTracking()
            .AnyAsync(x => x.Slug == slug, cancellationToken);

        if (slugExists)
        {
            throw new InvalidOperationException($"Section slug '{slug}' already exists.");
        }

        var entity = new Section
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _dbContext.Sections.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    public async Task<SectionResponse?> UpdateSectionAsync(Guid id, UpdateSectionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Sections
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            entity.Name = request.Name.Trim();
        }

        if (request.Description is not null)
        {
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        if (request.SortOrder.HasValue)
        {
            entity.SortOrder = request.SortOrder.Value;
        }

        if (request.IsActive.HasValue)
        {
            entity.IsActive = request.IsActive.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Slug) || !string.IsNullOrWhiteSpace(request.Name))
        {
            var slugSource = string.IsNullOrWhiteSpace(request.Slug) ? entity.Name : request.Slug;
            var slug = SlugHelper.GenerateSlug(slugSource);

            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new InvalidOperationException("Slug could not be generated.");
            }

            var slugExists = await _dbContext.Sections
                .AsNoTracking()
                .AnyAsync(x => x.Slug == slug && x.Id != id, cancellationToken);

            if (slugExists)
            {
                throw new InvalidOperationException($"Section slug '{slug}' already exists.");
            }

            entity.Slug = slug;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    private static SectionResponse MapToResponse(Section section)
    {
        return new SectionResponse
        {
            Id = section.Id,
            Name = section.Name,
            Slug = section.Slug,
            Description = section.Description,
            SortOrder = section.SortOrder,
            IsActive = section.IsActive
        };
    }
}
