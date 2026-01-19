using CodeQuizBackend.Core.Data.models;
using CodeQuizBackend.Execution.Models;
using CodeQuizBackend.Execution.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuizBackend.Execution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecutionController(ICodeRunnerFactory codeRunnerFactory) : ControllerBase
    {
        [HttpGet("languages")]
        public ActionResult<ApiResponse<IEnumerable<SupportedLanguage>>> GetSupportedLanguages()
        {
            var languages = codeRunnerFactory.GetSupportedLanguages();
            return Ok(new ApiResponse<IEnumerable<SupportedLanguage>> { Success = true, Data = languages });
        }

        [HttpPost("run")]
        public async Task<ActionResult<ApiResponse<CodeRunnerResult>>> RunCode([FromBody] RunCodeRequest request)
        {
            var codeRunner = codeRunnerFactory.Create(request.Language);
            var result = await codeRunner.RunCodeAsync(request.Code, new CodeRunnerOptions
            {
                Input = request.Input,
                ContainOutput = request.ContainOutput,
                ContainError = request.ContainError
            });
            return Ok(new ApiResponse<CodeRunnerResult> { Success = true, Data = result });
        }
    }
}
