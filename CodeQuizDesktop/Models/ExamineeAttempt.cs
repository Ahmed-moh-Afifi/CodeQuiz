using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class ExamineeAttempt
    {
        public required int Id { get; set; }
        public required DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } = null;
        public required int QuizId { get; set; }
        public required string ExamineeId { get; set; }
        public float? Grade { get; set; }
        public float? GradePercentage { get; set; }
        public required ExamineeQuiz Quiz { get; set; }
        public required List<Solution> Solutions { get; set; }
        public string Status
        {
            get
            {
                if (EndTime == null)
                {
                    return "Running";
                }
                else
                {
                    return "Completed";
                }
            }
        }
        public TimeSpan? Duration { get => (EndTime - StartTime); }

        public string DurationString
        {
            get
            {
                if (Duration.HasValue)
                {
                    if (Duration.Value.TotalHours >= 1)
                    {
                        return $"{(int)Duration.Value.TotalHours}h {(int)Duration.Value.Minutes}m {(int)Duration.Value.Seconds}s";
                    }
                    else if (Duration.Value.TotalMinutes >= 1)
                    {
                        return $"{(int)Duration.Value.TotalMinutes}m {(int)Duration.Value.Seconds}s";
                    }
                    else
                    {
                        return $"{(int)Duration.Value.Seconds}s";
                    }
                }
                return "-";
            }
        }
        
        public string SubmissionTimeString
        {
            get
            {
                if (EndTime.HasValue)
                {
                    return EndTime.Value.ToShortDateString() + " - " + EndTime.Value.ToShortTimeString();
                }
                return "-";
            }
        }

        public bool IsRunning { get => Status == "Running"; }
        public bool IsCompleted { get => Status == "Completed"; }
        public float? GradePercentageOverHundred { get => (GradePercentage/100);  }
        public required DateTime MaxEndTime { get; set; }
    }
}
