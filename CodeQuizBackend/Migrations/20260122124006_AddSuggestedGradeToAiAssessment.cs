using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeQuizBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestedGradeToAiAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "SuggestedGrade",
                table: "AiAssessments",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuggestedGrade",
                table: "AiAssessments");
        }
    }
}
