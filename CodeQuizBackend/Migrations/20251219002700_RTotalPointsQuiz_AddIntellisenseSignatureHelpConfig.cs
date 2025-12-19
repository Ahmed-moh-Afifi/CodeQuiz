using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeQuizBackend.Migrations
{
    /// <inheritdoc />
    public partial class RTotalPointsQuiz_AddIntellisenseSignatureHelpConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "Quizzes");

            migrationBuilder.AddColumn<bool>(
                name: "GlobalQuestionConfiguration_AllowIntellisense",
                table: "Quizzes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GlobalQuestionConfiguration_AllowSignatureHelp",
                table: "Quizzes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "QuestionConfiguration_AllowIntellisense",
                table: "Questions",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "QuestionConfiguration_AllowSignatureHelp",
                table: "Questions",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GlobalQuestionConfiguration_AllowIntellisense",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "GlobalQuestionConfiguration_AllowSignatureHelp",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "QuestionConfiguration_AllowIntellisense",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuestionConfiguration_AllowSignatureHelp",
                table: "Questions");

            migrationBuilder.AddColumn<float>(
                name: "TotalPoints",
                table: "Quizzes",
                type: "float",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
