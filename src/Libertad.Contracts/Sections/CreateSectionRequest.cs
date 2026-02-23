namespace Libertad.Contracts.Sections;

public class CreateSectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
