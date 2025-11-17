using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Authentication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodeQuizBackend.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUsersRepository usersRepository) : ControllerBase 
    {
        [HttpGet]
        [Route("{userId}")]
        [Authorize] // Protected endpoint (clients must provide a valid JWT)
        public async Task<ActionResult<UserDTO>> GetUser(string userId)
        {
            UserDTO? user = await usersRepository.GetUser(userId);
            return user != null ? Ok(user) : NotFound("User not found");
        }

        // Unprotected endpoint (no JWT needed)
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> SearchUsers(string query, DateTime? lastDate, string? lastId)
        {
            return Ok(await usersRepository.SearchUsers(query, lastDate, lastId));
        }

        [HttpPut]
        [Route("{userId}")]
        [Authorize()]
        public async Task<ActionResult> UpdateUser(string userId, [FromBody] UserDTO user)
        {
            await usersRepository.UpdateUser(user);
            return Ok();
        }

        [HttpGet]
        [Route("Username/{userName}/Available")]
        public async Task<ActionResult<bool>> IsUserNameAvailable(string userName)
        {
            return Ok(await usersRepository.IsUserNameAvailable(userName));
        }
    }
}
