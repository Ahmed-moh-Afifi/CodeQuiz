using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeQuizBackend.Migrations
{
    /// <inheritdoc />
    public partial class AttemptEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Attempts");

            migrationBuilder.AddColumn<string>(
                name: "EvaluatedBy",
                table: "Solutions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<float>(
                name: "ReceivedGrade",
                table: "Solutions",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvaluatedBy",
                table: "Solutions");

            migrationBuilder.DropColumn(
                name: "ReceivedGrade",
                table: "Solutions");

            migrationBuilder.AddColumn<float>(
                name: "Grade",
                table: "Attempts",
                type: "float",
                nullable: true);
        }
    }
}
