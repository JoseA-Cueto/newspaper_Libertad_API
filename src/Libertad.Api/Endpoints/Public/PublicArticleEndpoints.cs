using Libertad.Application.Articles.Public;
using Libertad.Contracts.Articles;
using Microsoft.AspNetCore.Mvc;

namespace Libertad.Api.Endpoints.Public;

public static class PublicArticleEndpoints
{
    public static void MapPublicArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public")
            .WithName("Public");

        group.MapGet("/home", GetHome)
            .WithName("GetHome")
            .WithSummary("Get latest published articles (last 7 days)")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/sections/{slug}", GetBySectionSlug)
            .WithName("GetBySectionSlug")
            .WithSummary("Get published articles by section")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/articles/{slug}", GetArticleBySlug)
            .WithName("GetArticleBySlug")
            .WithSummary("Get a published article by slug")
            .Produces<ArticleDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/archive", GetArchive)
            .WithName("GetArchive")
            .WithSummary("Get archived articles (older than 7 days)")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetHome(
        [FromServices] IArticlePublicService publicService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await publicService.GetHomeAsync(page, pageSize, days: 7, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetBySectionSlug(
        [FromServices] IArticlePublicService publicService,
        [FromRoute] string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await publicService.GetBySectionSlugAsync(slug, page, pageSize, days: 7, cancellationToken);

            if (result is null)
            {
                return Results.NotFound(new { error = $"Section '{slug}' not found." });
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetArticleBySlug(
        [FromServices] IArticlePublicService publicService,
        [FromRoute] string slug,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await publicService.GetPublicArticleBySlugAsync(slug, cancellationToken);

            if (result is null)
            {
                return Results.NotFound(new { error = $"Article '{slug}' not found or not published." });
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetArchive(
        [FromServices] IArticlePublicService publicService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await publicService.GetArchiveAsync(page, pageSize, olderThanDays: 7, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
