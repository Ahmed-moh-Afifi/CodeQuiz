using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Quiz.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeQuizBackend.Core.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Quiz.Models.Quiz> Quizzes { get; set; }
        public DbSet<Quiz.Models.Question> Questions { get; set; }
        public DbSet<Quiz.Models.Attempt> Attempts { get; set; }
        public DbSet<Quiz.Models.Solution> Solutions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Quiz.Models.Quiz>().OwnsOne(q => q.GlobalQuestionConfiguration);
            builder.Entity<Quiz.Models.Question>().OwnsOne(q => q.QuestionConfiguration);
            builder.Entity<Quiz.Models.Question>().OwnsMany(q => q.TestCases);
            builder.Entity<Quiz.Models.Solution>().OwnsMany(s => s.EvaluationResults, er => er.OwnsOne(q => q.TestCase));
        }
    }
}
