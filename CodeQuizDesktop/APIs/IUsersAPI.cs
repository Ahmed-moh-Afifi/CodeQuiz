using CodeQuizDesktop.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.APIs
{
    public interface IUsersAPI
    {
        [Get("/Users")]
        public Task<Models.ApiResponse<User>> GetUser();

        [Get("/Users/Search")]
        public Task<Models.ApiResponse<List<User>>> Search([Query] string query);

        [Get("/Users/Username/{username}/Available")]
        public Task<Models.ApiResponse<bool>> IsUsernameAvailable(string username);

        [Put("/Users/{userId}")]
        public Task<Models.ApiResponse<User>> UpdateUser(string userId, [Body] User updatedUser);
    }
}
