using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Quiz.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CodeQuizBackend.Core.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Quiz.Models.Quiz> Quizzes { get; set; }
        public DbSet<Quiz.Models.Question> Questions { get; set; }
        public DbSet<Quiz.Models.Attempt> Attempts { get; set; }
        public DbSet<Quiz.Models.Solution> Solutions { get; set; }
        public DbSet<AiAssessment> AiAssessments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Quiz.Models.Quiz>().OwnsOne(q => q.GlobalQuestionConfiguration);
            builder.Entity<Quiz.Models.Question>().OwnsOne(q => q.QuestionConfiguration);
            builder.Entity<Quiz.Models.Question>().OwnsMany(q => q.TestCases);
            builder.Entity<Quiz.Models.Solution>().OwnsMany(s => s.EvaluationResults, er => er.OwnsOne(q => q.TestCase));

            // AI Assessment configuration
            builder.Entity<AiAssessment>()
                .HasOne(a => a.Solution)
                .WithOne(s => s.AiAssessment)
                .HasForeignKey<AiAssessment>(a => a.SolutionId);

            // Store Flags as JSON with proper value comparer
            var stringListComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            builder.Entity<AiAssessment>()
                .Property(a => a.Flags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                .Metadata.SetValueComparer(stringListComparer);
        }
    }
}
