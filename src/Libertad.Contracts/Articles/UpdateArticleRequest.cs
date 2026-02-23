namespace Libertad.Contracts.Articles;

public class UpdateArticleRequest
{
    public Guid? SectionId { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? ContentMarkdown { get; set; }
}
