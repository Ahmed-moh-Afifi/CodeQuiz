using CodeQuizDesktop.Models;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Services
{
    public interface IQuizDialogService
    {
        Task<NewQuestionModel?> ShowAddQuestionDialogAsync(string? globalLanguage = null);
        Task<NewQuestionModel?> ShowEditQuestionDialogAsync(NewQuestionModel existingQuestion, string? globalLanguage = null);
    }
}
