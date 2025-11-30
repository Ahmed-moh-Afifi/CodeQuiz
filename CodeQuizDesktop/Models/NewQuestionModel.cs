using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class NewQuestionModel : BaseObservableModel
    {
        private int order;
        private string editorCode = string.Empty;
        public required string Statement { get; set; }
        public required string EditorCode
        {
            get => editorCode;
            set
            {
                editorCode = value;
                OnPropertyChanged();
            }
        }
        public QuestionConfiguration? QuestionConfiguration { get; set; }
        public required List<TestCase> TestCases { get; set; }
        public required int Order
        {
            get => order;
            set
            {
                order = value;
                OnPropertyChanged();
            }
        }
        public required float Points { get; set; }
    }
}
