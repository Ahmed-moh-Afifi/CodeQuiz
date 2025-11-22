using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class NewQuizModel
    {
        public required string Title { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required TimeSpan Duration { get; set; }
        public required string ExaminerId { get; set; }
        public required QuestionConfiguration GlobalQuestionConfiguration { get; set; }
        public required bool AllowMultipleAttempts { get; set; }
        public required List<NewQuestionModel> Questions { get; set; }
    }
}
