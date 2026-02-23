using Libertad.Domain.Enums;

namespace Libertad.Contracts.Articles;

public class ReviewQueueItemResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string? SectionName { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public ArticleStatus Status { get; set; }
}
