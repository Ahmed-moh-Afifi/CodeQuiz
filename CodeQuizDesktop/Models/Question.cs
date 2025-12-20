using CodeQuizDesktop.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class Question : BaseObservableModel
    {
        private int order;

        public required int Id { get; set; }
        public required string Statement { get; set; }
        public required string EditorCode { get; set; }
        public required QuestionConfiguration QuestionConfiguration { get; set; }
        public required List<TestCase> TestCases { get; set; }
        public required int QuizId { get; set; }
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

        public NewQuestionModel QuestionToModel
        {
            get
            {
                return new NewQuestionModel
                {
                    Statement = this.Statement,
                    EditorCode = this.EditorCode,
                    QuestionConfiguration = this.QuestionConfiguration,
                    TestCases = this.TestCases,
                    Order = this.Order,
                    Points = this.Points
                };
            }
        }
    }
}
