using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class SeedExistingSiteContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "About",
                columns: new[] { "Id", "Description", "Image", "Title" },
                values: new object[] { 1, "", "/images/Image.png", "Sobre" });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Name", "Order", "Proficiency" },
                values: new object[,]
                {
                    { 1, "Português", 0, "" },
                    { 2, "Inglês", 1, "" }
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "CvUrl", "Email", "GithubUrl", "LinkedInUrl", "Name", "ProfessionalTitle", "ProfileImage" },
                values: new object[] { 1, "#projetos", "alyssonsb.dev@gmail.com", "#", "#", "Alysson Serafim Brito", "Desenvolvedor de Jogos e Softwares", "/images/Image.png" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "About",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
