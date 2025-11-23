using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class BeginAttemptRequest
    {
        public required string QuizCode { get; set; }
    }
}
