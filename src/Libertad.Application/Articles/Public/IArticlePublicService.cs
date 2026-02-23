using Libertad.Contracts.Articles;
using Libertad.Contracts.Pagination;

namespace Libertad.Application.Articles.Public;

public interface IArticlePublicService
{
    Task<PagedResponse<ArticleSummaryResponse>> GetHomeAsync(
        int page = 1,
        int pageSize = 10,
        int days = 7,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<ArticleSummaryResponse>?> GetBySectionSlugAsync(
        string sectionSlug,
        int page = 1,
        int pageSize = 10,
        int days = 7,
        CancellationToken cancellationToken = default);

    Task<ArticleDetailResponse?> GetPublicArticleBySlugAsync(
        string articleSlug,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<ArticleSummaryResponse>> GetArchiveAsync(
        int page = 1,
        int pageSize = 10,
        int olderThanDays = 7,
        CancellationToken cancellationToken = default);
}
