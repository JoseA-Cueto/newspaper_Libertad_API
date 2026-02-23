using Libertad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Libertad.Infrastructure.Persistence.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("sections");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        var seedCreatedAt = new DateTime(2026, 2, 23, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new Section
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Política",
                Slug = "politica",
                Description = null,
                SortOrder = 1,
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = null
            },
            new Section
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Sociedad",
                Slug = "sociedad",
                Description = null,
                SortOrder = 2,
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = null
            },
            new Section
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Cultura",
                Slug = "cultura",
                Description = null,
                SortOrder = 3,
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = null
            },
            new Section
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Economía",
                Slug = "economia",
                Description = null,
                SortOrder = 4,
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = null
            },
            new Section
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Educación y Civismo",
                Slug = "educacion-y-civismo",
                Description = null,
                SortOrder = 5,
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = null
            },
            new Section
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Diáspora",
                Slug = "diaspora",
                Description = null,
                SortOrder = 6,
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = null
            },
            new Section
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Opinión",
                Slug = "opinion",
                Description = null,
                SortOrder = 7,
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = null
            },
            new Section
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Análisis y Datos",
                Slug = "analisis-y-datos",
                Description = null,
                SortOrder = 8,
                IsActive = true,
                CreatedAt = seedCreatedAt,
                UpdatedAt = null
            }
        );
    }
}
