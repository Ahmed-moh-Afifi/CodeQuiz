using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Quiz.Models.DTOs;

namespace CodeQuizBackend.Quiz.Models
{
    public class Quiz
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required TimeSpan Duration { get; set; }
        public required string Code { get; set; }
        public required string ExaminerId { get; set; }
        public required QuestionConfiguration GlobalQuestionConfiguration { get; set; }
        public required bool AllowMultipleAttempts { get; set; }

        public virtual User Examiner { get; set; } = null!;
        public virtual List<Question> Questions { get; set; } = [];
        public virtual List<Attempt> Attempts { get; set; } = [];

        public ExaminerQuiz ToExaminerQuiz()
        {
            return new ExaminerQuiz
            {
                Id = Id,
                Title = Title,
                StartDate = StartDate,
                EndDate = EndDate,
                Duration = Duration,
                Code = Code,
                ExaminerId = ExaminerId,
                GlobalQuestionConfiguration = GlobalQuestionConfiguration,
                AllowMultipleAttempts = AllowMultipleAttempts,
                Questions = Questions.Select(q => q.ToDTO(q.QuestionConfiguration ?? GlobalQuestionConfiguration)).ToList()
            };
        }

        public ExamineeQuiz ToExamineeQuiz()
        {
            return new ExamineeQuiz
            {
                Id = Id,
                Title = Title,
                StartDate = StartDate,
                EndDate = EndDate,
                Duration = Duration,
                Code = Code,
                ExaminerId = ExaminerId,
                GlobalQuestionConfiguration = GlobalQuestionConfiguration,
                AllowMultipleAttempts = AllowMultipleAttempts,
                Questions = Questions.Select(q => q.ToDTO(q.QuestionConfiguration ?? GlobalQuestionConfiguration)).ToList(),
                Examiner = Examiner.ToDTO()
            };
        }
    }
}
