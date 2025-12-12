using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class Solution
    {
        public required int Id { get; set; }
        public required string Code { get; set; }
        public required int QuestionId { get; set; }
        public required int AttemptId { get; set; }
        public string? EvaluatedBy { get; set; }
        public float? ReceivedGrade { get; set; }
        public List<EvaluationResult>? EvaluationResults { get; set; }
    }
}
