using Libertad.Application.Abstractions;
using Libertad.Contracts.Articles;
using Libertad.Contracts.Pagination;
using Libertad.Domain.Entities;
using Libertad.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Libertad.Application.Articles.Public;

public class ArticlePublicService : IArticlePublicService
{
    private readonly IApplicationDbContext _dbContext;

    public ArticlePublicService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<ArticleSummaryResponse>> GetHomeAsync(
        int page = 1,
        int pageSize = 10,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 10;
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var query = _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.Status == ArticleStatus.Published && x.PublishedAt != null && x.PublishedAt >= cutoffDate)
            .Include(x => x.Section);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var articles = await query
            .OrderByDescending(x => x.IssueDate.HasValue ? x.IssueDate.Value : DateTime.MinValue)
            .ThenByDescending(x => x.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = articles.Select(MapToSummaryResponse).ToList();

        return new PagedResponse<ArticleSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<PagedResponse<ArticleSummaryResponse>?> GetBySectionSlugAsync(
        string sectionSlug,
        int page = 1,
        int pageSize = 10,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 10;
        }

        var section = await _dbContext.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == sectionSlug && x.IsActive, cancellationToken);

        if (section is null)
        {
            return null;
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var query = _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.SectionId == section.Id && x.Status == ArticleStatus.Published && x.PublishedAt != null && x.PublishedAt >= cutoffDate)
            .Include(x => x.Section);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var articles = await query
            .OrderByDescending(x => x.IssueDate.HasValue ? x.IssueDate.Value : DateTime.MinValue)
            .ThenByDescending(x => x.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = articles.Select(MapToSummaryResponse).ToList();

        return new PagedResponse<ArticleSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<ArticleDetailResponse?> GetPublicArticleBySlugAsync(
        string articleSlug,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.Slug == articleSlug && x.Status == ArticleStatus.Published && x.PublishedAt != null)
            .Include(x => x.Section)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return MapToDetailResponse(entity, entity.Section);
    }

    public async Task<PagedResponse<ArticleSummaryResponse>> GetArchiveAsync(
        int page = 1,
        int pageSize = 10,
        int olderThanDays = 7,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 10;
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

        var query = _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.Status == ArticleStatus.Published && x.PublishedAt != null && x.PublishedAt < cutoffDate)
            .Include(x => x.Section);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var articles = await query
            .OrderByDescending(x => x.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = articles.Select(MapToSummaryResponse).ToList();

        return new PagedResponse<ArticleSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    private static ArticleSummaryResponse MapToSummaryResponse(Article article)
    {
        return new ArticleSummaryResponse
        {
            Id = article.Id,
            Title = article.Title,
            Subtitle = article.Subtitle,
            Slug = article.Slug,
            Status = article.Status,
            SectionId = article.SectionId,
            SectionName = article.Section?.Name,
            SectionSlug = article.Section?.Slug,
            AuthorId = article.AuthorId,
            IssueDate = article.IssueDate,
            PublishedAt = article.PublishedAt,
            CreatedAt = article.CreatedAt
        };
    }

    private static ArticleDetailResponse MapToDetailResponse(Article article, Section? section)
    {
        return new ArticleDetailResponse
        {
            Id = article.Id,
            Title = article.Title,
            Subtitle = article.Subtitle,
            Slug = article.Slug,
            ContentMarkdown = article.ContentMarkdown,
            Status = article.Status,
            SectionId = article.SectionId,
            SectionName = section?.Name,
            SectionSlug = section?.Slug,
            AuthorId = article.AuthorId,
            IssueDate = article.IssueDate,
            SubmittedAt = article.SubmittedAt,
            PublishedAt = article.PublishedAt,
            ArchivedAt = article.ArchivedAt,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }
}
