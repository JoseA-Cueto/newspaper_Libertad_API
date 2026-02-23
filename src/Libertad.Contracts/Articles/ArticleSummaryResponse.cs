using Libertad.Domain.Enums;

namespace Libertad.Contracts.Articles;

public class ArticleSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Slug { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; }
    public Guid SectionId { get; set; }
    public string? SectionName { get; set; }
    public string? SectionSlug { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
