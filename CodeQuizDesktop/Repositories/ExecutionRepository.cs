using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public class ExecutionRepository(IExecutionAPI executionAPI) : IExecutionRepository
    {
        public async Task<CodeRunnerResult> RunCode(RunCodeRequest runCodeRequest)
        {
            try
            {
                return (await executionAPI.RunCode(runCodeRequest)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }
    }
}
