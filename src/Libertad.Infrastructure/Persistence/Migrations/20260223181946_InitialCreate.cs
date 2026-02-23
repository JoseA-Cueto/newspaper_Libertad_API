using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Libertad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "articles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    content_markdown = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_articles", x => x.id);
                    table.ForeignKey(
                        name: "FK_articles_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "review_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    editor_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_comments_articles_article_id",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "sections",
                columns: new[] { "id", "created_at", "description", "is_active", "name", "slug", "sort_order", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Política", "politica", 1, null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Sociedad", "sociedad", 2, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Cultura", "cultura", 3, null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Economía", "economia", 4, null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Educación y Civismo", "educacion-y-civismo", 5, null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Diáspora", "diaspora", 6, null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Opinión", "opinion", 7, null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Análisis y Datos", "analisis-y-datos", 8, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_articles_issue_date_status",
                table: "articles",
                columns: new[] { "issue_date", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_articles_published_at",
                table: "articles",
                column: "published_at");

            migrationBuilder.CreateIndex(
                name: "IX_articles_section_id_published_at",
                table: "articles",
                columns: new[] { "section_id", "published_at" });

            migrationBuilder.CreateIndex(
                name: "IX_articles_slug",
                table: "articles",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_review_comments_article_id",
                table: "review_comments",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "IX_sections_slug",
                table: "sections",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "review_comments");

            migrationBuilder.DropTable(
                name: "articles");

            migrationBuilder.DropTable(
                name: "sections");
        }
    }
}
