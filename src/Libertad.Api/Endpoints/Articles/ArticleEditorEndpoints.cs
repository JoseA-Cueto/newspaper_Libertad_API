using Libertad.Application.Articles.Services;
using Libertad.Application.Articles.Workflow;
using Libertad.Contracts.Articles;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Libertad.Api.Endpoints.Articles;

public static class ArticleEditorEndpoints
{
    public static void MapArticleEditorEndpoints(this IEndpointRouteBuilder app)
    {
        var authorGroup = app.MapGroup("/api/articles")
            .WithName("Articles")
            .RequireAuthorization("Author");

        // Author: submit article
        authorGroup.MapPost("/{id:guid}/submit", SubmitArticle)
            .WithName("SubmitArticle")
            .WithSummary("Submit an article for review (author)")
            .Produces<ArticleDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        var reviewGroup = app.MapGroup("/api/review/articles")
            .WithName("ArticleReview")
            .RequireAuthorization("Editor");

        // Editor: request changes
        reviewGroup.MapPost("/{id:guid}/request-changes", RequestChanges)
            .WithName("RequestChanges")
            .WithSummary("Request changes to an article (editor)")
            .Produces<ArticleDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // Editor: approve article
        reviewGroup.MapPost("/{id:guid}/approve", ApproveArticle)
            .WithName("ApproveArticle")
            .WithSummary("Approve an article for publication (editor)")
            .Produces<ArticleDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // Editor: publish article
        reviewGroup.MapPost("/{id:guid}/publish", PublishArticle)
            .WithName("PublishArticle")
            .WithSummary("Publish an approved article (editor)")
            .Produces<ArticleDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SubmitArticle(
        [FromServices] IArticleWorkflowService workflowService,
        HttpContext httpContext,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var authorId = GetAuthorId(httpContext);
            if (string.IsNullOrWhiteSpace(authorId))
            {
                return Results.Unauthorized();
            }

            var result = await workflowService.SubmitArticleAsync(id, authorId, cancellationToken);

            if (result is null)
            {
                return Results.NotFound(new { error = $"Article with id '{id}' not found or does not belong to you." });
            }

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> RequestChanges(
        [FromServices] IArticleWorkflowService workflowService,
        HttpContext httpContext,
        [FromRoute] Guid id,
        [FromBody] RequestChangesArticleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var editorId = GetEditorId(httpContext);
            if (string.IsNullOrWhiteSpace(editorId))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return Results.BadRequest(new { error = "Comment is required when requesting changes." });
            }

            var result = await workflowService.RequestChangesAsync(id, editorId, request.Comment, cancellationToken);

            if (result is null)
            {
                return Results.NotFound(new { error = $"Article with id '{id}' not found." });
            }

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> ApproveArticle(
        [FromServices] IArticleWorkflowService workflowService,
        HttpContext httpContext,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var editorId = GetEditorId(httpContext);
            if (string.IsNullOrWhiteSpace(editorId))
            {
                return Results.Unauthorized();
            }

            var result = await workflowService.ApproveArticleAsync(id, editorId, cancellationToken);

            if (result is null)
            {
                return Results.NotFound(new { error = $"Article with id '{id}' not found." });
            }

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> PublishArticle(
        [FromServices] IArticleWorkflowService workflowService,
        HttpContext httpContext,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var editorId = GetEditorId(httpContext);
            if (string.IsNullOrWhiteSpace(editorId))
            {
                return Results.Unauthorized();
            }

            var result = await workflowService.PublishArticleAsync(id, editorId, cancellationToken);

            if (result is null)
            {
                return Results.NotFound(new { error = $"Article with id '{id}' not found." });
            }

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static string? GetAuthorId(HttpContext httpContext)
    {
        return httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private static string? GetEditorId(HttpContext httpContext)
    {
        return httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
