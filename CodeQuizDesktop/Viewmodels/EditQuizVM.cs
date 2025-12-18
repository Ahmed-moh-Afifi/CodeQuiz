using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Viewmodels
{
    public class EditQuizVM : BaseViewModel, IQueryAttributable
    {
        private ExaminerQuiz quiz;
        public ExaminerQuiz Quiz
        {
            get { return quiz; }
            set
            {
                quiz = value;
                OnPropertyChanged();
            }
        }
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("quiz") && query["quiz"] is ExaminerQuiz receivedQuiz)
            {
                Quiz = receivedQuiz;
                System.Diagnostics.Debug.WriteLine($"Clicked: {Quiz.Title}");
            }
        }
    }
}
