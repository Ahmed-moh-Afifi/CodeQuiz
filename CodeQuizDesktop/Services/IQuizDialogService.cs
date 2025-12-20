using CodeQuizDesktop.Models;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Services
{
    public interface IQuizDialogService
    {
        Task<NewQuestionModel?> ShowAddQuestionDialogAsync();
        Task<NewQuestionModel?> ShowEditQuestionDialogAsync(NewQuestionModel existingQuestion);
    }
}
