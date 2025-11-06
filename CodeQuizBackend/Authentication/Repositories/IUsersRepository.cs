using CodeQuizBackend.Authentication.Models.DTOs;

namespace CodeQuizBackend.Authentication.Repositories
{
    public interface IUsersRepository
    {
        public Task<IEnumerable<UserDTO>> SearchUsers(string query, DateTime? lastDate, string? lastId);
        public Task<UserDTO> GetUser(string id);
        public Task UpdateUser(UserDTO user);
        public Task<bool> IsUserNameAvailable(string userName);
    }
}
