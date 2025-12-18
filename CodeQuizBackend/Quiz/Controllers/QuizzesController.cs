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
    public class QuizzesController(IQuizzesService quizzesService) : ControllerBase
    {
        /// <summary>
        /// Creates a new quiz
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ExaminerQuiz>>> CreateQuiz([FromBody] NewQuizModel newQuizModel)
        {
            var validationErrors = newQuizModel.Validate();
            if (validationErrors.Count > 0)
            {
                throw new BadRequestException(string.Join('\n', validationErrors));
            }

            var quiz = await quizzesService.CreateQuiz(newQuizModel);
            return Created($"/api/Quizzes/code/{quiz.Code}", new ApiResponse<ExaminerQuiz>
            {
                Success = true,
                Data = quiz,
                Message = "Quiz created successfully"
            });
        }

        /// <summary>
        /// Updates an existing quiz
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ExaminerQuiz>>> UpdateQuiz(int id, [FromBody] ExaminerQuiz quiz)
        {
            if (id != quiz.Id)
            {
                throw new BadRequestException("Quiz ID mismatch");
            }

            var updatedQuiz = await quizzesService.UpdateQuiz(quiz);
            return Ok(new ApiResponse<ExaminerQuiz>
            {
                Success = true,
                Data = updatedQuiz,
                Message = "Quiz updated successfully"
            });
        }

        /// <summary>
        /// Gets all quizzes for the authenticated user
        /// </summary>
        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<ExaminerQuiz>>>> GetUserQuizzes()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User ID not found in token");
            }

            var quizzes = await quizzesService.GetUserQuizzes(userId);
            return Ok(new ApiResponse<List<ExaminerQuiz>>
            {
                Success = true,
                Data = quizzes
            });
        }

        /// <summary>
        /// Gets all quizzes for a specific user by ID
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<ExaminerQuiz>>>> GetUserQuizzesById(string userId)
        {
            var quizzes = await quizzesService.GetUserQuizzes(userId);
            return Ok(new ApiResponse<List<ExaminerQuiz>>
            {
                Success = true,
                Data = quizzes
            });
        }

        /// <summary>
        /// Deletes a quiz by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteQuiz(int id)
        {
            await quizzesService.DeleteQuiz(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Quiz deleted successfully"
            });
        }

        /// <summary>
        /// Gets a quiz by its unique code (for examinees to join)
        /// </summary>
        [HttpGet("code/{code}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ExamineeQuiz>>> GetQuizByCode(string code)
        {
            var quiz = await quizzesService.GetQuizByCode(code);
            return Ok(new ApiResponse<ExamineeQuiz>
            {
                Success = true,
                Data = quiz
            });
        }


        [HttpGet("{quizId}/attempts")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<ExaminerAttempt>>>> GetQuizAttempts(int quizId)
        {
            var attempts = await quizzesService.GetQuizAttempts(quizId);
            return Ok(new ApiResponse<List<ExaminerAttempt>>
            {
                Success = true,
                Data = attempts
            });
        }
    }
}
