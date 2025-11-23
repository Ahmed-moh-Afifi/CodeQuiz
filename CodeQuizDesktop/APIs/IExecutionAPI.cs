using CodeQuizDesktop.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.APIs
{
    public interface IExecutionAPI
    {
        [Post("/Execution/run")]
        public Task<Models.ApiResponse<CodeRunnerResult>> RunCode([Body] RunCodeRequest runCodeRequest);
    }
}
