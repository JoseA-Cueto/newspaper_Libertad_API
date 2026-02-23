using Libertad.Contracts.Sections;

namespace Libertad.Application.Sections.Services;

public interface ISectionService
{
    Task<IReadOnlyList<SectionResponse>> GetActiveSectionsAsync(CancellationToken cancellationToken = default);
    Task<SectionResponse> CreateSectionAsync(CreateSectionRequest request, CancellationToken cancellationToken = default);
    Task<SectionResponse?> UpdateSectionAsync(Guid id, UpdateSectionRequest request, CancellationToken cancellationToken = default);
}
