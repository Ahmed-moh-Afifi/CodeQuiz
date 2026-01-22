using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class ExamineeQuiz
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
        public bool ShowAiFeedbackToStudents { get; set; }
        public required List<Question> Questions { get; set; }
        public required User Examiner { get; set; }
        public required int QustionsCount { get; set; }
        public required float TotalPoints { get; set; }

        /// <summary>
        /// StartDate converted to local time for UI display
        /// </summary>
        public DateTime StartDateLocal => StartDate.ToLocalTime();

        /// <summary>
        /// EndDate converted to local time for UI display
        /// </summary>
        public DateTime EndDateLocal => EndDate.ToLocalTime();

        /// <summary>
        /// StartDate as formatted string in local time
        /// </summary>
        public string StartDateString => StartDate.ToLocalTime().ToShortDateString() + " - " + StartDate.ToLocalTime().ToShortTimeString();

        /// <summary>
        /// EndDate as formatted string in local time
        /// </summary>
        public string EndDateString => EndDate.ToLocalTime().ToShortDateString() + " - " + EndDate.ToLocalTime().ToShortTimeString();

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
    }
}
