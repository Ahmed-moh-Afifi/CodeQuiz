using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class JoinQuizVM : BaseViewModel
    {
        public ICommand ReturnCommand { get => new Command(ReturnToPreviousPage); }

        private async void ReturnToPreviousPage()
        {
            await Shell.Current.GoToAsync("///MainPage");

        }

        public JoinQuizVM()
        {
        }
    }
}
