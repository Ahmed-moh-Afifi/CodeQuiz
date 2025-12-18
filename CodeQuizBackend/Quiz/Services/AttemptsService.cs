using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Execution.Services;
using CodeQuizBackend.Quiz.Exceptions;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Models;
using CodeQuizBackend.Quiz.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CodeQuizBackend.Quiz.Services
{
    public class AttemptsService(ApplicationDbContext dbContext, IEvaluator evaluator, IHubContext<AttemptsHub> attemptsHubContext) : IAttemptsService
    {
        public async Task<ExamineeAttempt> BeginAttempt(string quizCode, string examineeId)
        {
            var attempt = await dbContext.Attempts
                .Where(a => a.Quiz.Code == quizCode && a.ExamineeId == examineeId && a.EndTime == null)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(q => q.Solutions)
                .FirstOrDefaultAsync();

            if (attempt == null)
            {
                var hasExpiredAttempt = await dbContext.Attempts
                    .AnyAsync(a => a.Quiz.Code == quizCode && a.ExamineeId == examineeId && a.EndTime != null);

                var quiz = await dbContext.Quizzes
                .Where(q => q.Code == quizCode)
                .Include(q => q.Examiner)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();

                if (quiz == null)
                {
                    throw new ResourceNotFoundException("Quiz not found. Please check the code and try again.");
                }

                if (DateTime.Now < quiz.StartDate)
                {
                    throw new QuizNotActiveException("This quiz has not started yet. Please wait until the scheduled start time.");
                }

                if (DateTime.Now > quiz.EndDate)
                {
                    throw new QuizNotActiveException("This quiz has ended and is no longer accepting new attempts.");
                }

                if (hasExpiredAttempt && !quiz.AllowMultipleAttempts)
                {
                    throw new MultipleAttemptsNotAllowedException();
                }

                attempt = new()
                {
                    Id = 0,
                    StartTime = DateTime.Now,
                    QuizId = quiz.Id,
                    ExamineeId = examineeId,
                    Quiz = quiz,
                    Solutions = quiz.Questions.Select(q => new Solution
                    {
                        Id = 0,
                        AttemptId = 0,
                        QuestionId = q.Id,
                        Code = q.EditorCode,
                    }).ToList()
                };
                dbContext.Attempts.Add(attempt);
                await dbContext.SaveChangesAsync();
                await attemptsHubContext.Clients.All.SendAsync("AttemptCreated", attempt.ToExaminerAttempt(), attempt.ToExamineeAttempt());
            }

            return attempt.ToExamineeAttempt();
        }

        public async Task<List<ExamineeAttempt>> GetExamineeAttempts(string examineeId)
        {
            var examineeAttempts = await dbContext.Attempts
                .Where(a => a.ExamineeId == examineeId)
                .Include(a => a.Solutions)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Select(a => a.ToExamineeAttempt())
                .ToListAsync();
            return examineeAttempts;
        }

        public async Task<ExamineeAttempt> SubmitAttempt(int attemptId)
        {
            var attempt = await dbContext.Attempts
                .Where(a => a.Id == attemptId)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Examiner)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(a => a.Solutions)
                .FirstOrDefaultAsync()
                ?? throw new ResourceNotFoundException("Attempt not found.");

            attempt.EndTime ??= DateTime.Now;
            await dbContext.SaveChangesAsync();
            await attemptsHubContext.Clients.All.SendAsync("AttemptUpdated", attempt.ToExaminerAttempt(), attempt.ToExamineeAttempt());
            await EvaluateAttempt(attempt.Id);
            return attempt.ToExamineeAttempt();
        }

        public async Task<SolutionDTO> UpdateSolution(SolutionDTO solution)
        {
            var sol = await dbContext.Solutions.FindAsync(solution.Id)
                ?? throw new ResourceNotFoundException("Solution not found.");

            sol.Code = solution.Code;
            await dbContext.SaveChangesAsync();
            return sol.ToDTO();
        }

        public async Task EvaluateAttempt(int attemptId)
        {
            var attempt = await dbContext.Attempts
                .Where(a => a.Id == attemptId)
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(a => a.Solutions)
                .FirstOrDefaultAsync()
                ?? throw new ResourceNotFoundException("Attempt not found.");

            if (attempt.EndTime == null)
            {
                throw new AttemptNotSubmittedException();
            }

            GradeAttemptSolutions(attempt);
            await dbContext.SaveChangesAsync();
            await attemptsHubContext.Clients.All.SendAsync("AttemptUpdated", attempt.ToExaminerAttempt(), attempt.ToExamineeAttempt());
        }

        private void GradeAttemptSolutions(Attempt attempt)
        {
            foreach (var solution in attempt.Solutions)
            {
                var question = attempt.Quiz.Questions.First(q => q.Id == solution.QuestionId);
                if (!question.TestCases.IsNullOrEmpty())
                {
                    var correctSolution = TestCasesPassed(attempt.Quiz, question, solution);
                    solution.ReceivedGrade = correctSolution ? question.Points : 0;
                    solution.EvaluatedBy = "System";
                }
            }
        }

        private bool TestCasesPassed(Quiz.Models.Quiz quiz, Question question, Solution solution)
        {
            var questionConfig = question.QuestionConfiguration ?? quiz.GlobalQuestionConfiguration;
            foreach (var testCase in question.TestCases)
            {
                var result = evaluator.EvaluateAsync(questionConfig.Language, solution.Code, testCase).Result;
                if (!result.IsSuccessful)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
