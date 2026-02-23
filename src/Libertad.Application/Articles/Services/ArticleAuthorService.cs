using Libertad.Application.Abstractions;
using Libertad.Application.Common;
using Libertad.Contracts.Articles;
using Libertad.Contracts.Pagination;
using Libertad.Domain.Entities;
using Libertad.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Libertad.Application.Articles.Services;

public class ArticleAuthorService : IArticleAuthorService
{
    private readonly IApplicationDbContext _dbContext;

    public ArticleAuthorService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArticleDetailResponse> CreateArticleAsync(
        string authorId,
        CreateArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Article title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ContentMarkdown))
        {
            throw new InvalidOperationException("Article content is required.");
        }

        var sectionExists = await _dbContext.Sections
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.SectionId && x.IsActive, cancellationToken);

        if (!sectionExists)
        {
            throw new InvalidOperationException($"Section '{request.SectionId}' does not exist or is inactive.");
        }

        var slug = await GenerateUniqueSlugAsync(request.Title, null, cancellationToken);

        var entity = new Article
        {
            Id = Guid.NewGuid(),
            SectionId = request.SectionId,
            AuthorId = authorId,
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle?.Trim(),
            Slug = slug,
            ContentMarkdown = request.ContentMarkdown.Trim(),
            Status = ArticleStatus.Draft,
            IssueDate = null,
            SubmittedAt = null,
            PublishedAt = null,
            ArchivedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _dbContext.Articles.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(entity, null);
    }

    public async Task<ArticleDetailResponse?> UpdateArticleAsync(
        Guid articleId,
        string authorId,
        UpdateArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Articles
            .FirstOrDefaultAsync(x => x.Id == articleId && x.AuthorId == authorId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != ArticleStatus.Draft && entity.Status != ArticleStatus.ChangesRequested)
        {
            throw new InvalidOperationException(
                $"Cannot edit article with status '{entity.Status}'. Only Draft and ChangesRequested articles can be edited.");
        }

        if (request.SectionId.HasValue && request.SectionId.Value != entity.SectionId)
        {
            var sectionExists = await _dbContext.Sections
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.SectionId.Value && x.IsActive, cancellationToken);

            if (!sectionExists)
            {
                throw new InvalidOperationException($"Section '{request.SectionId}' does not exist or is inactive.");
            }

            entity.SectionId = request.SectionId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var newTitle = request.Title.Trim();
            if (newTitle != entity.Title)
            {
                entity.Title = newTitle;
                var newSlug = await GenerateUniqueSlugAsync(newTitle, articleId, cancellationToken);
                entity.Slug = newSlug;
            }
        }

        if (request.Subtitle is not null)
        {
            entity.Subtitle = string.IsNullOrWhiteSpace(request.Subtitle) ? null : request.Subtitle.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.ContentMarkdown))
        {
            entity.ContentMarkdown = request.ContentMarkdown.Trim();
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var section = await _dbContext.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == entity.SectionId, cancellationToken);

        return MapToDetailResponse(entity, section);
    }

    public async Task<PagedResponse<ArticleSummaryResponse>> GetMyArticlesAsync(
        string authorId,
        int page = 1,
        int pageSize = 10,
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

        var totalItems = await _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.AuthorId == authorId)
            .CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var articles = await _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.AuthorId == authorId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Section)
            .ToListAsync(cancellationToken);

        var items = articles.Select(a => MapToSummaryResponse(a)).ToList();

        return new PagedResponse<ArticleSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<ArticleDetailResponse?> GetMyArticleByIdAsync(
        Guid articleId,
        string authorId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Articles
            .AsNoTracking()
            .Include(x => x.Section)
            .FirstOrDefaultAsync(x => x.Id == articleId && x.AuthorId == authorId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return MapToDetailResponse(entity, entity.Section);
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string title,
        Guid? excludeArticleId,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.GenerateSlug(title);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            throw new InvalidOperationException("Slug could not be generated from title.");
        }

        var slug = baseSlug;
        var counter = 1;

        while (true)
        {
            var slugExists = await _dbContext.Articles
                .AsNoTracking()
                .AnyAsync(x => x.Slug == slug && (excludeArticleId == null || x.Id != excludeArticleId), cancellationToken);

            if (!slugExists)
            {
                break;
            }

            counter++;
            slug = $"{baseSlug}-{counter}";
        }

        return slug;
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
