using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class QuestionConfiguration
    {
        public required string Language { get; set; }
        public required bool AllowExecution { get; set; }
        public required bool ShowOutput { get; set; }
        public required bool ShowError { get; set; }
    }
}
