using CodeQuizBackend.Core.Data.models;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Quiz.Models.DTOs;
using CodeQuizBackend.Quiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuizBackend.Quiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttemptsController(IAttemptsService attemptsService) : ControllerBase
    {
        /// <summary>
        /// Begins a new quiz attempt for an examinee
        /// </summary>
        [HttpPost("begin")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ExamineeAttempt>>> BeginAttempt([FromBody] BeginAttemptRequest request)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User ID not found in token");
            }

            var attempt = await attemptsService.BeginAttempt(request.QuizCode, userId);
            return Created($"/api/Attempts/{attempt.Id}", new ApiResponse<ExamineeAttempt>
            {
                Success = true,
                Data = attempt,
                Message = "Attempt started successfully"
            });
        }

        /// <summary>
        /// Submits a quiz attempt
        /// </summary>
        [HttpPost("{attemptId}/submit")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ExamineeAttempt>>> SubmitAttempt(int attemptId)
        {
            var attempt = await attemptsService.SubmitAttempt(attemptId);
            return Ok(new ApiResponse<ExamineeAttempt>
            {
                Success = true,
                Data = attempt,
                Message = "Attempt submitted successfully"
            });
        }

        [HttpPut("solutions")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SolutionDTO>>> UpdateSolution([FromBody] SolutionDTO solution)
        {
            var updatedSolution = await attemptsService.UpdateSolution(solution);
            return Ok(new ApiResponse<SolutionDTO>()
            {
                Success = true,
                Data = updatedSolution,
                Message = "Solution updated successfully"
            });
        }

        /// <summary>
        /// Gets all attempts for the authenticated user
        /// </summary>
        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<ExamineeAttempt>>>> GetUserAttempts()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User ID not found in token");
            }

            var attempts = await attemptsService.GetExamineeAttempts(userId);
            return Ok(new ApiResponse<List<ExamineeAttempt>>
            {
                Success = true,
                Data = attempts
            });
        }
    }

    public class BeginAttemptRequest
    {
        public required string QuizCode { get; set; }
    }
}
