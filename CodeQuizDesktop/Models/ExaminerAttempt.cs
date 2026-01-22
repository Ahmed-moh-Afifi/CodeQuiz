using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class ExaminerAttempt
    {
        public required int Id { get; set; }
        public required DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } = null;
        public required int QuizId { get; set; }
        public required string ExamineeId { get; set; }
        public float? Grade { get; set; }
        public required float TotalPoints { get; set; }
        public required List<Solution> Solutions { get; set; }
        public required User Examinee { get; set; }

        /// <summary>
        /// StartTime converted to local time for UI display
        /// </summary>
        public DateTime StartTimeLocal => StartTime.ToLocalTime();

        /// <summary>
        /// EndTime converted to local time for UI display
        /// </summary>
        public DateTime? EndTimeLocal => EndTime?.ToLocalTime();

        /// <summary>
        /// StartTime as formatted string in local time
        /// </summary>
        public string StartTimeString
        {
            get
            {
                var localStartTime = StartTime.ToLocalTime();
                return localStartTime.ToShortDateString() + " - " + localStartTime.ToShortTimeString();
            }
        }

        /// <summary>
        /// EndTime as formatted string in local time
        /// </summary>
        public string SubmissionTimeString
        {
            get
            {
                if (EndTime.HasValue)
                {
                    // Convert UTC time to local time for display
                    var localEndTime = EndTime.Value.ToLocalTime();
                    return localEndTime.ToShortDateString() + " - " + localEndTime.ToShortTimeString();
                }
                return "-";
            }
        }

        /// <summary>
        /// Whether this attempt has any solutions flagged as suspicious by AI assessment.
        /// </summary>
        public bool HasSuspiciousSolutions => Solutions.Any(s => s.AiAssessment != null && !s.AiAssessment.IsValid);

        /// <summary>
        /// Whether this attempt has AI assessments on any solution.
        /// </summary>
        public bool HasAiAssessments => Solutions.Any(s => s.AiAssessment != null);

        /// <summary>
        /// Count of solutions with suspicious AI assessments.
        /// </summary>
        public int SuspiciousSolutionCount => Solutions.Count(s => s.AiAssessment != null && !s.AiAssessment.IsValid);

        /// <summary>
        /// Whether all solutions have been graded.
        /// </summary>
        public bool IsFullyGraded => Solutions.All(s => s.ReceivedGrade.HasValue);

        /// <summary>
        /// Whether at least one solution has been graded.
        /// </summary>
        public bool IsPartiallyGraded => Solutions.Any(s => s.ReceivedGrade.HasValue) && !IsFullyGraded;

        /// <summary>
        /// Whether no solutions have been graded yet.
        /// </summary>
        public bool IsPendingGrading => !Solutions.Any(s => s.ReceivedGrade.HasValue);

        /// <summary>
        /// The grading status text for display.
        /// </summary>
        public string GradingStatusText => IsFullyGraded ? "Graded" : IsPartiallyGraded ? "Partial" : "Pending";

        /// <summary>
        /// The primary status to show (prioritizes suspicious over grading status).
        /// </summary>
        public string PrimaryStatus => HasSuspiciousSolutions ? "Suspicious" : GradingStatusText;
    }
}
