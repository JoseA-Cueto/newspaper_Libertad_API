namespace Libertad.Domain.Entities;

public class ReviewComment
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public string EditorId { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Article? Article { get; set; }
}
