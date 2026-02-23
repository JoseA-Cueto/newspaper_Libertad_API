namespace Libertad.Contracts.Articles;

public class CreateArticleRequest
{
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ContentMarkdown { get; set; } = string.Empty;
}
