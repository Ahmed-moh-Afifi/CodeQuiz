using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class ExaminerQuiz
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
        public required List<Question> Questions { get; set; }
        public required int QustionsCount { get; set; }
        public required int AttemptsCount { get; set; }
        public required int SubmittedAttemptsCount { get; set; }
        public required float AverageAttemptScore { get; set; }
        public required float TotalPoints { get; set; }
        public string StartDateString { get => StartDate.ToShortDateString() + " - " + StartDate.ToShortTimeString(); }
        public string EndDateString { get => EndDate.ToShortDateString() + " - " + EndDate.ToShortTimeString(); }
        public string DurationString
        {
            get
            {
                if (Duration.TotalHours >= 1)
                {
                    return $"{(int)Duration.TotalHours}h {(int)Duration.Minutes}m {(int)Duration.Seconds}s";
                }
                else if (Duration.TotalMinutes >= 1)
                {
                    return $"{(int)Duration.TotalMinutes}m {(int)Duration.Seconds}s";
                }
                else
                {
                    return $"{(int)Duration.Seconds}s";
                }
            }
        }
        public string Status
        {
            get
            {
                if (StartDate > DateTime.Now)
                    return "Not Started";
                else if (EndDate < DateTime.Now)
                    return "Ended";
                else
                    return "Running";
            }
        }

        public NewQuizModel QuizToModel
        {
            get
            {
                return new NewQuizModel
                {
                    Title = this.Title,
                    StartDate = this.StartDate,
                    EndDate = this.EndDate,
                    Duration = this.Duration,
                    ExaminerId = this.ExaminerId,
                    GlobalQuestionConfiguration = this.GlobalQuestionConfiguration,
                    AllowMultipleAttempts = this.AllowMultipleAttempts,
                    Questions = this.Questions.Select(q => q.QuestionToModel).ToList() 
                };
            }
        }

    }
}
