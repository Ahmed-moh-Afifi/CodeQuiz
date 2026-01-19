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

        public float AveragePercentage => TotalPoints > 0 ? (AverageAttemptScore / TotalPoints) * 100 : 0;

        // Convert UTC dates to local time for display
        public string StartDateString { get => StartDate.ToLocalTime().ToShortDateString() + " - " + StartDate.ToLocalTime().ToShortTimeString(); }
        public string EndDateString { get => EndDate.ToLocalTime().ToShortDateString() + " - " + EndDate.ToLocalTime().ToShortTimeString(); }
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
                // Compare UTC times with current UTC time for accurate status
                var nowUtc = DateTime.UtcNow;
                if (StartDate > nowUtc)
                    return "Upcoming";
                else if (EndDate < nowUtc)
                    return "Ended";
                else
                    return "Running";
            }
        }
        public bool IsUpcoming { get => Status == "Upcoming"; }
        public bool IsEnded { get => Status == "Ended"; }
        public bool IsRunning { get => Status == "Running"; }

        public NewQuizModel QuizToModel
        {
            get
            {
                return new NewQuizModel
                {
                    Title = this.Title,
                    // Convert UTC to local time for editing in UI
                    StartDate = this.StartDate.ToLocalTime(),
                    EndDate = this.EndDate.ToLocalTime(),
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
