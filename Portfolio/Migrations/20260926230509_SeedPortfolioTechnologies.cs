using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class SeedPortfolioTechnologies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Technologies", columns: new[] { "Id", "Category", "Icon", "Name", "Order", "Url" },
                values: new object[,]
                {
{ 1, "Design", "/images/technologies/figma.png", "Figma", 0, null },
{ 2, "Design", "/images/technologies/canva.png", "Canva", 1, null },
{ 3, "Design", "/images/technologies/photoshop.png", "Adobe Photoshop", 2, null },
{ 4, "Design", "/images/technologies/illustrator.png", "Adobe Illustrator", 3, null },
{ 5, "Design", "/images/technologies/xd.png", "Adobe XD", 4, null },
{ 6, "Backend e Banco de Dados", "/images/technologies/csharp.png", "C#", 5, null },
{ 7, "Backend e Banco de Dados", "/images/technologies/dotnet.png", ".NET", 6, null },
{ 8, "Backend e Banco de Dados", "/images/technologies/sql.png", "SQL", 7, null },
{ 9, "Front-End", "/images/technologies/html.png", "HTML", 8, null },
{ 10, "Front-End", "/images/technologies/css.png", "CSS", 9, null },
{ 11, "Front-End", "/images/technologies/javascript.png", "JavaScript", 10, null },
{ 12, "Front-End", "/images/technologies/blazor.png", "Blazor", 11, null },
{ 13, "DevOps e Desenvolvimento", "/images/technologies/git.png", "Git", 12, null },
{ 14, "DevOps e Desenvolvimento", "/images/technologies/github.png", "GitHub", 13, null },
{ 15, "DevOps e Desenvolvimento", "/images/technologies/docker.png", "Docker", 14, null },
{ 16, "DevOps e Desenvolvimento", "/images/technologies/unity.png", "Unity", 15, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Technologies",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
        }
    }
}
