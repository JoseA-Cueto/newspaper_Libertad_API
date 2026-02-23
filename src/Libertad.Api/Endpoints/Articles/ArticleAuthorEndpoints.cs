using Libertad.Application.Articles.Services;
using Libertad.Contracts.Articles;

namespace Libertad.Api.Endpoints.Articles;

public static class ArticleAuthorEndpoints
{
    private const string DevAuthorId = "dev-author";

    public static void MapArticleAuthorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/articles")
            .WithName("Articles");

        group.MapPost("/", CreateArticle)
            .WithName("CreateArticle")
            .WithSummary("Create a new article as author")
            .Produces<ArticleDetailResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateArticle)
            .WithName("UpdateArticle")
            .WithSummary("Update an article as author")
            .Produces<ArticleDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/mine", GetMyArticles)
            .WithName("GetMyArticles")
            .WithSummary("Get all articles by current author")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/mine/{id:guid}", GetMyArticleById)
            .WithName("GetMyArticleById")
            .WithSummary("Get a specific article by id")
            .Produces<ArticleDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateArticle(
        IArticleAuthorService articleService,
        HttpContext httpContext,
        CreateArticleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var authorId = GetAuthorId(httpContext);

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { error = "Article title is required." });
            }

            if (string.IsNullOrWhiteSpace(request.ContentMarkdown))
            {
                return Results.BadRequest(new { error = "Article content is required." });
            }

            var result = await articleService.CreateArticleAsync(authorId, request, cancellationToken);
            return Results.Created($"/api/articles/{result.Id}", result);
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

    private static async Task<IResult> UpdateArticle(
        IArticleAuthorService articleService,
        HttpContext httpContext,
        Guid id,
        UpdateArticleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var authorId = GetAuthorId(httpContext);

            var result = await articleService.UpdateArticleAsync(id, authorId, request, cancellationToken);

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

    private static async Task<IResult> GetMyArticles(
        IArticleAuthorService articleService,
        HttpContext httpContext,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authorId = GetAuthorId(httpContext);

            var result = await articleService.GetMyArticlesAsync(authorId, page, pageSize, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetMyArticleById(
        IArticleAuthorService articleService,
        HttpContext httpContext,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var authorId = GetAuthorId(httpContext);

            var result = await articleService.GetMyArticleByIdAsync(id, authorId, cancellationToken);

            if (result is null)
            {
                return Results.NotFound(new { error = $"Article with id '{id}' not found or does not belong to you." });
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static string GetAuthorId(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers["X-Author-Id"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(header))
        {
            return header;
        }

        if (httpContext.RequestServices.GetService(typeof(IWebHostEnvironment)) is IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                return DevAuthorId;
            }
        }

        return DevAuthorId;
    }
}
