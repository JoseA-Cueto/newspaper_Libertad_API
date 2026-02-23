using Libertad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Libertad.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.SectionId)
            .HasColumnName("section_id")
            .IsRequired();

        builder.Property(x => x.AuthorId)
            .HasColumnName("author_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Subtitle)
            .HasColumnName("subtitle")
            .HasMaxLength(250);

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(x => x.ContentMarkdown)
            .HasColumnName("content_markdown")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IssueDate)
            .HasColumnName("issue_date");

        builder.Property(x => x.SubmittedAt)
            .HasColumnName("submitted_at");

        builder.Property(x => x.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(x => x.ArchivedAt)
            .HasColumnName("archived_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(x => x.Section)
            .WithMany(x => x.Articles)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.IssueDate, x.Status });

        builder.HasIndex(x => x.PublishedAt);

        builder.HasIndex(x => new { x.SectionId, x.PublishedAt });
    }
}
