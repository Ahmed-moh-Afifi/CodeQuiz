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
        public required List<Solution> Solutions { get; set; }
        public required User Examinee { get; set; }
    }
}
