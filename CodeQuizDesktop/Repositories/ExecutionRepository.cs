using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Models;

namespace CodeQuizDesktop.Repositories;

public class ExecutionRepository(IExecutionAPI executionAPI) : IExecutionRepository
{
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
