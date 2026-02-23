using Libertad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Libertad.Infrastructure.Persistence.Configurations;

public class ReviewCommentConfiguration : IEntityTypeConfiguration<ReviewComment>
{
    public void Configure(EntityTypeBuilder<ReviewComment> builder)
    {
        builder.ToTable("review_comments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.ArticleId)
            .HasColumnName("article_id")
            .IsRequired();

        builder.Property(x => x.EditorId)
            .HasColumnName("editor_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(x => x.Article)
            .WithMany(x => x.ReviewComments)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ArticleId);
    }
}
