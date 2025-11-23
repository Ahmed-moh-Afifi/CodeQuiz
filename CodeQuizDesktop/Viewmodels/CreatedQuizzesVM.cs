using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class CreatedQuizzesVM : BaseViewModel
    {
        public ICommand CreateQuizCommand { get => new Command(OpenCreateQuizPage); }

        private async void OpenCreateQuizPage()
        {
            await Shell.Current.GoToAsync("///CreateQuizPage");
        }

        public ICommand ViewQuizCommand { get => new Command(OpenExaminerViewQuizPage); }

        private async void OpenExaminerViewQuizPage()
        {
            await Shell.Current.GoToAsync("///ExaminerViewQuizPage");
        }


        public CreatedQuizzesVM()
        {
           
        }
    }
}
