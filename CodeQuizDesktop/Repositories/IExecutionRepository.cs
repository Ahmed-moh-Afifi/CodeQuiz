using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface IExecutionRepository
    {
        public Task<IEnumerable<SupportedLanguage>> GetSupportedLanguages();
        public Task<CodeRunnerResult> RunCode(RunCodeRequest runCodeRequest);
    }
}
