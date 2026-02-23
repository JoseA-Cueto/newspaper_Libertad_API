namespace Libertad.Contracts.Sections;

public class UpdateSectionRequest
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}
