using CodeQuizBackend.Core.Data.models;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Models.DTOs;
using CodeQuizBackend.Quiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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

        /// <summary>
        /// Updates a solution's code (student saving their work)
        /// </summary>
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
        /// Updates a solution's grade (instructor grading)
        /// </summary>
        [HttpPut("solutions/grade")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SolutionDTO>>> UpdateSolutionGrade([FromBody] UpdateSolutionGradeRequest request)
        {
            var updatedSolution = await attemptsService.UpdateSolutionGrade(request);
            return Ok(new ApiResponse<SolutionDTO>()
            {
                Success = true,
                Data = updatedSolution,
                Message = "Solution grade updated successfully"
            });
        }

        /// <summary>
        /// Batch updates grades and feedback for multiple solutions at once.
        /// Sends a single email notification to the examinee with all changes.
        /// </summary>
        [HttpPut("solutions/grades/batch")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<SolutionDTO>>>> BatchUpdateSolutionGrades([FromBody] BatchUpdateSolutionGradesRequest request)
        {
            var updatedSolutions = await attemptsService.BatchUpdateSolutionGrades(request);
            return Ok(new ApiResponse<List<SolutionDTO>>()
            {
                Success = true,
                Data = updatedSolutions,
                Message = "Solution grades updated successfully"
            });
        }

        /// <summary>
        /// Re-runs AI assessment for a specific solution.
        /// </summary>
        [HttpPost("solutions/{solutionId}/ai-reassess")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SolutionDTO>>> RerunAiAssessment(int solutionId)
        {
            var solution = await attemptsService.RerunAiAssessment(solutionId);
            return Ok(new ApiResponse<SolutionDTO>()
            {
                Success = true,
                Data = solution,
                Message = "AI reassessment queued successfully"
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

        /// <summary>
        /// Gets a specific attempt by ID for the examiner view.
        /// </summary>
        [HttpGet("{attemptId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ExaminerAttempt>>> GetAttemptById(int attemptId)
        {
            var attempt = await attemptsService.GetAttemptById(attemptId);
            return Ok(new ApiResponse<ExaminerAttempt>
            {
                Success = true,
                Data = attempt
            });
        }

    }

    public class BeginAttemptRequest
    {
        public required string QuizCode { get; set; }
    }
}
