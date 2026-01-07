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
    }
}
