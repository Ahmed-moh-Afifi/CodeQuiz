using CodeQuizDesktop.Models;
using Refit;

namespace CodeQuizDesktop.APIs
{
    public interface IAiAPI
    {
        [Post("/Ai/generate-testcases")]
        Task<Models.ApiResponse<List<GeneratedTestCase>>> GenerateTestCases([Body] GenerateTestCasesRequest request);
    }
}
