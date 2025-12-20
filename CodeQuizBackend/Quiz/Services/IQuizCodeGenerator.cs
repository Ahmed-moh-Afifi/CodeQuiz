using System.Threading.Tasks;

namespace CodeQuizBackend.Quiz.Services
{
    public interface IQuizCodeGenerator
    {
        Task<string> GenerateUniqueQuizCode();
    }
}
