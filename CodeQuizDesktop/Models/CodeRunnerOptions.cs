using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class CodeRunnerOptions
    {
        public bool ContainOutput { get; set; } = false;
        public bool ContainError { get; set; } = false;
        public List<string> Input { get; set; } = [];
    }
}
