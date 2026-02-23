using Libertad.Contracts.Articles;
using Libertad.Contracts.Pagination;
using Libertad.Domain.Enums;

namespace Libertad.Application.Articles.Services;

public interface IArticleAuthorService
{
    Task<ArticleDetailResponse> CreateArticleAsync(
        string authorId,
        CreateArticleRequest request,
        CancellationToken cancellationToken = default);

    Task<ArticleDetailResponse?> UpdateArticleAsync(
        Guid articleId,
        string authorId,
        UpdateArticleRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<ArticleSummaryResponse>> GetMyArticlesAsync(
        string authorId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<ArticleDetailResponse?> GetMyArticleByIdAsync(
        Guid articleId,
        string authorId,
        CancellationToken cancellationToken = default);
}
