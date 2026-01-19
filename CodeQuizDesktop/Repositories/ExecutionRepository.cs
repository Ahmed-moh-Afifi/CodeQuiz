using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Models;

namespace CodeQuizDesktop.Repositories;

public class ExecutionRepository(IExecutionAPI executionAPI) : IExecutionRepository
{
    public async Task<IEnumerable<SupportedLanguage>> GetSupportedLanguages()
    {
        try
        {
            return (await executionAPI.GetSupportedLanguages()).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to get supported languages.");
        }
    }

    public async Task<CodeRunnerResult> RunCode(RunCodeRequest runCodeRequest)
    {
        try
        {
            return (await executionAPI.RunCode(runCodeRequest)).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to execute code.");
        }
    }
}
