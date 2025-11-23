using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface IUsersRepository
    {
        public Task<User> GetUser(string userId);
        public List<User> Search(string query);
        public Task<bool> IsUsernameAvailable(string username);
        public Task<User> UpdateUser(string userId, User updatedUser);
    }
}
