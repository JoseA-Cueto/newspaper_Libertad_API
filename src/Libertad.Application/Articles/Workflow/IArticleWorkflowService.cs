using Libertad.Contracts.Articles;
using Libertad.Domain.Enums;

namespace Libertad.Application.Articles.Workflow;

public interface IArticleWorkflowService
{
    Task<ArticleDetailResponse?> SubmitArticleAsync(
        Guid articleId,
        string authorId,
        CancellationToken cancellationToken = default);

    Task<ArticleDetailResponse?> RequestChangesAsync(
        Guid articleId,
        string editorId,
        string? comment,
        CancellationToken cancellationToken = default);

    Task<ArticleDetailResponse?> ApproveArticleAsync(
        Guid articleId,
        string editorId,
        CancellationToken cancellationToken = default);

    Task<ArticleDetailResponse?> PublishArticleAsync(
        Guid articleId,
        string editorId,
        CancellationToken cancellationToken = default);
}
