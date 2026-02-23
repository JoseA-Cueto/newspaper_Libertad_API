using Libertad.Domain.Enums;

namespace Libertad.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public DateTime? IssueDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Section? Section { get; set; }
    public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
}
