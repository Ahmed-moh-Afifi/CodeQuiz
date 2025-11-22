using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class CodeRunnerResult
    {
        public bool Success { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }
    }
}
