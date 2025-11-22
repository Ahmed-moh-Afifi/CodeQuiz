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
    }
}
