using CodeQuizDesktop.Viewmodels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class TestCase : BaseObservableModel
    {
        private int testCaseNumber;
        public required int TestCaseNumber
        {
            get => testCaseNumber;
            set
            {
                testCaseNumber = value;
                OnPropertyChanged();
            }
        }
        public required List<string> Input { get; set; }
        public required string ExpectedOutput { get; set; }
        public string InputInOneString
        {
            get => string.Join('\n', Input);
            set => Input = value.Split(['\n', ' ', '\r']).ToList();
        }
    }
}
