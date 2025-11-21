using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeQuizBackend.Migrations
{
    /// <inheritdoc />
    public partial class QuizQuestionPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "TotalPoints",
                table: "Quizzes",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Points",
                table: "Questions",
                type: "float",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Questions");
        }
    }
}
