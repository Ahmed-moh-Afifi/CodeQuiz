using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class JoinedQuizzesVM : BaseViewModel
    {
        public ICommand JoinQuizCommand { get => new Command(OpenJoinQuizPage); }

        private async void OpenJoinQuizPage()
        {
            await Shell.Current.GoToAsync("///JoinQuizPage");
        }

        public JoinedQuizzesVM()
        {
        }
    }
}
