using Libertad.Application.Sections.Services;
using Libertad.Contracts.Sections;
using Microsoft.AspNetCore.Mvc;

namespace Libertad.Api.Endpoints.Sections;

public static class SectionEndpoints
{
    public static void MapSectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sections")
            .WithName("Sections");

        group.MapGet("/", GetSections)
            .WithName("GetSections")
            .WithSummary("Get all active sections")
            .Produces<List<SectionResponse>>();

        group.MapPost("/", CreateSection)
            .WithName("CreateSection")
            .WithSummary("Create a new section (editor only)")
            .Produces<SectionResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateSection)
            .WithName("UpdateSection")
            .WithSummary("Update a section (editor only)")
            .Produces<SectionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetSections(
        [FromServices] ISectionService sectionService,
        CancellationToken cancellationToken)
    {
        try
        {
            var sections = await sectionService.GetActiveSectionsAsync(cancellationToken);
            return Results.Ok(sections);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> CreateSection(
        [FromServices] ISectionService sectionService,
        [FromBody] CreateSectionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { error = "Section name is required." });
            }

            var result = await sectionService.CreateSectionAsync(request, cancellationToken);
            return Results.Created($"/api/sections/{result.Id}", result);
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

    private static async Task<IResult> UpdateSection(
        [FromServices] ISectionService sectionService,
        [FromRoute] Guid id,
        [FromBody] UpdateSectionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sectionService.UpdateSectionAsync(id, request, cancellationToken);

            if (result is null)
            {
                return Results.NotFound(new { error = $"Section with id '{id}' not found." });
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
}
