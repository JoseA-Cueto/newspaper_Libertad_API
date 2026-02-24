using Libertad.Api.Endpoints.Articles;
using Libertad.Api.Endpoints.Sections;
using Libertad.Api.Endpoints.Public;
using Libertad.Application.Abstractions;
using Libertad.Application.Articles.Public;
using Libertad.Application.Articles.Services;
using Libertad.Application.Articles.Workflow;
using Libertad.Application.Sections.Services;
using Libertad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<LibertadDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<LibertadDbContext>());

builder.Services.AddScoped<ISectionService, SectionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/api/error");
}

app.UseHttpsRedirection();
app.UseCors("DefaultCors");

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    timestampUtc = DateTime.UtcNow
}));

app.MapGet("/api/error", () => Results.Problem("An unexpected error occurred."));

app.MapSectionEndpoints();
app.MapArticleAuthorEndpoints();
app.MapArticleEditorEndpoints();
app.MapPublicArticleEndpoints();

app.Run();
