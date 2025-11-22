using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class RunCodeRequest
    {
        public required string Language { get; set; }
        public required string Code { get; set; }
        public required List<string> Input { get; set; }
        public required bool ContainOutput { get; set; }
        public required bool ContainError { get; set; }
    }
}
