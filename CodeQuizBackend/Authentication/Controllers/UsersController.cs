using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Authentication.Repositories;
using CodeQuizBackend.Core.Data.models;
using CodeQuizBackend.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuizBackend.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUsersRepository usersRepository) : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [Authorize] // Protected endpoint (clients must provide a valid JWT)
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetUser()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User ID not found in token");
            }

            UserDTO? user = await usersRepository.GetUser(userId);
            return user != null ? Ok(new ApiResponse<UserDTO>() { Success = true, Data = user }) : NotFound("User not found");
        }

        // Unprotected endpoint (no JWT needed)
        [HttpGet]
        [Route("Search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDTO>>>> SearchUsers(string query, DateTime? lastDate, string? lastId)
        {
            return Ok(new ApiResponse<IEnumerable<UserDTO>>() { Success = true, Data = await usersRepository.SearchUsers(query, lastDate, lastId) });
        }

        [HttpPut]
        [Route("{userId}")]
        [Authorize()]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUser(string userId, [FromBody] UserDTO user)
        {
            await usersRepository.UpdateUser(user);
            return Ok(new ApiResponse<object>() { Success = true });
        }

        [HttpGet]
        [Route("Username/{userName}/Available")]
        public async Task<ActionResult<ApiResponse<bool>>> IsUserNameAvailable(string userName)
        {
            return Ok(new ApiResponse<bool>() { Success = true, Data = await usersRepository.IsUserNameAvailable(userName) });
        }
    }
}
