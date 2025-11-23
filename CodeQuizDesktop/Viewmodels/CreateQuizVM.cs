using CodeQuizDesktop.Views;
using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeQuizDesktop.Viewmodels
{
    public class CreateQuizVM : BaseViewModel
    {
        public ICommand AddQuestionCommand { get; set; }
        public ICommand QuizSettingsCommand { get; set; }

        private readonly IPopupService popupService;


        //private async void AddQuestion()
        //{
        //    //await Application.Current!.Windows[0].Navigation.PushModalAsync(new AddQuestionDialog());

        //}
        public ICommand ReturnCommand { get => new Command(ReturnToPreviousPage); }

        private async void ReturnToPreviousPage()
        {
            await Shell.Current.GoToAsync("///MainPage");
            
        }

        public async void OpenAddQuestionDialog()
        {
            var result = await popupService.ShowPopupAsync<AddQuestionDialog, string?>(Shell.Current, new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(20),
                    StrokeThickness = 0
                }
            });
            await Application.Current!.MainPage!.DisplayPromptAsync("Result", $"You selected: {result}", "OK");
        }
        public async void OpenQuizSettingsDialog()
        {
            var result = await popupService.ShowPopupAsync<QuizSettingsDialog, string?>(Shell.Current, new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(20),
                    StrokeThickness = 0
                }
            });
            await Application.Current!.MainPage!.DisplayPromptAsync("Result", $"You selected: {result}", "OK");
        }

        public CreateQuizVM(IPopupService popupService)
        {
            this.popupService = popupService;
            AddQuestionCommand = new Command(OpenAddQuestionDialog);
            QuizSettingsCommand = new Command(OpenQuizSettingsDialog);

        }
    }
}
