using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class EvaluationResult
    {
        public required TestCase TestCase { get; set; }
        public required string Output { get; set; }
        public required bool IsSuccessful { get; set; }
    }
}
