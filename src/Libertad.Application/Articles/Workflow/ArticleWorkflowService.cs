using Libertad.Application.Abstractions;
using Libertad.Contracts.Articles;
using Libertad.Domain.Entities;
using Libertad.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Libertad.Application.Articles.Workflow;

public class ArticleWorkflowService : IArticleWorkflowService
{
    private readonly IApplicationDbContext _dbContext;

    public ArticleWorkflowService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ArticleDetailResponse?> SubmitArticleAsync(
        Guid articleId,
        string authorId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Articles
            .Include(x => x.Section)
            .FirstOrDefaultAsync(x => x.Id == articleId && x.AuthorId == authorId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != ArticleStatus.Draft && entity.Status != ArticleStatus.ChangesRequested)
        {
            throw new InvalidOperationException(
                $"Cannot submit article with status '{entity.Status}'. Only Draft and ChangesRequested articles can be submitted.");
        }

        entity.Status = ArticleStatus.Submitted;
        entity.SubmittedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(entity, entity.Section);
    }

    public async Task<ArticleDetailResponse?> RequestChangesAsync(
        Guid articleId,
        string editorId,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Articles
            .Include(x => x.Section)
            .FirstOrDefaultAsync(x => x.Id == articleId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != ArticleStatus.Submitted && entity.Status != ArticleStatus.InReview)
        {
            throw new InvalidOperationException(
                $"Cannot request changes for article with status '{entity.Status}'. Only Submitted and InReview articles can have changes requested.");
        }

        entity.Status = ArticleStatus.ChangesRequested;
        entity.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(comment))
        {
            var reviewComment = new ReviewComment
            {
                Id = Guid.NewGuid(),
                ArticleId = articleId,
                EditorId = editorId,
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.ReviewComments.Add(reviewComment);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(entity, entity.Section);
    }

    public async Task<ArticleDetailResponse?> ApproveArticleAsync(
        Guid articleId,
        string editorId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Articles
            .Include(x => x.Section)
            .FirstOrDefaultAsync(x => x.Id == articleId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != ArticleStatus.Submitted && entity.Status != ArticleStatus.InReview)
        {
            throw new InvalidOperationException(
                $"Cannot approve article with status '{entity.Status}'. Only Submitted and InReview articles can be approved.");
        }

        entity.Status = ArticleStatus.Approved;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(entity, entity.Section);
    }

    public async Task<ArticleDetailResponse?> PublishArticleAsync(
        Guid articleId,
        string editorId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Articles
            .Include(x => x.Section)
            .FirstOrDefaultAsync(x => x.Id == articleId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != ArticleStatus.Approved)
        {
            throw new InvalidOperationException(
                $"Cannot publish article with status '{entity.Status}'. Only Approved articles can be published.");
        }

        var issueDate = await CalculateIssueDateAsync(cancellationToken);

        entity.Status = ArticleStatus.Published;
        entity.PublishedAt = DateTime.UtcNow;
        entity.IssueDate = issueDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(entity, entity.Section);
    }

    private async Task<DateTime> CalculateIssueDateAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var publishedTodayCount = await _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.PublishedAt != null && x.IssueDate.HasValue && x.IssueDate.Value.Date == today)
            .CountAsync(cancellationToken);

        const int dailyQuota = 5;

        if (publishedTodayCount < dailyQuota)
        {
            return today;
        }

        return today.AddDays(1);
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
