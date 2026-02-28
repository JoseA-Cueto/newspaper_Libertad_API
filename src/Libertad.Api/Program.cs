using Libertad.Api.Endpoints.Articles;
using Libertad.Api.Endpoints.Auth;
using Libertad.Api.Endpoints.Sections;
using Libertad.Api.Endpoints.Public;
using Libertad.Application.Abstractions;
using Libertad.Application.Articles.Public;
using Libertad.Application.Articles.Services;
using Libertad.Application.Articles.Workflow;
using Libertad.Application.Sections.Services;
using Libertad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token"
    });

});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<LibertadDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<LibertadDbContext>());

builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IArticlePublicService, ArticlePublicService>();
builder.Services.AddScoped<IArticleAuthorService, ArticleAuthorService>();
builder.Services.AddScoped<IArticleWorkflowService, ArticleWorkflowService>();

var jwtIssuer = builder.Configuration["Auth:Jwt:Issuer"]
    ?? throw new InvalidOperationException("Auth:Jwt:Issuer not configured.");
var jwtAudience = builder.Configuration["Auth:Jwt:Audience"]
    ?? throw new InvalidOperationException("Auth:Jwt:Audience not configured.");
var jwtSigningKey = builder.Configuration["Auth:Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Auth:Jwt:SigningKey not configured.");

if (jwtSigningKey.Length < 32)
{
    throw new InvalidOperationException("Auth:Jwt:SigningKey must be at least 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Author", policy => policy.RequireRole("Author"));
    options.AddPolicy("Editor", policy => policy.RequireRole("Editor"));
});

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
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/api/error");
}

app.UseHttpsRedirection();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    timestampUtc = DateTime.UtcNow
}));

app.MapGet("/api/error", () => Results.Problem("An unexpected error occurred."));

app.MapAuthEndpoints();
app.MapSectionEndpoints();
app.MapArticleAuthorEndpoints();
app.MapArticleEditorEndpoints();
app.MapPublicArticleEndpoints();

app.Run();
