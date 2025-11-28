using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class TestCase
    {
        public required int TestCaseNumber { get; set; }
        public required List<string> Input { get; set; }
        public required string ExpectedOutput { get; set; }
        public string InputInOneString
        {
            get => string.Join('\n', Input);
            set => Input = value.Split('\n').ToList();
        }
    }
}
