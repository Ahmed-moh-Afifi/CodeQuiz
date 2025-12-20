using CodeQuizDesktop.Models;
using CodeQuizDesktop.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Services
{
    public class QuizDialogService : IQuizDialogService
    {
        private readonly IPopupService _popupService;

        public QuizDialogService(IPopupService popupService)
        {
            _popupService = popupService;
        }

        public async Task<NewQuestionModel?> ShowAddQuestionDialogAsync()
        {
            var result = await _popupService.ShowPopupAsync<AddQuestionDialog, NewQuestionModel?>(Shell.Current, new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(20),
                    StrokeThickness = 0
                }
            });
            return result?.Result;
        }

        public async Task<NewQuestionModel?> ShowEditQuestionDialogAsync(NewQuestionModel existingQuestion)
        {
            var popup = new AddQuestionDialog(existingQuestion);
            var result = await Shell.Current.CurrentPage.ShowPopupAsync<NewQuestionModel?>(popup);
            NewQuestionModel? value = result?.Result;
            return value;
        }
    }
}
