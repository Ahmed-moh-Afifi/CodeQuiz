using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public class UsersRepository(IUsersAPI usersAPI) : IUsersRepository
    {
        public async Task<User> GetUser()
        {
            try
            {
                return (await usersAPI.GetUser()).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<bool> IsUsernameAvailable(string username)
        {
            try
            {
                return (await usersAPI.IsUsernameAvailable(username)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<List<User>> Search(string query)
        {
            try
            {
                return (await usersAPI.Search(query)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<User> UpdateUser(string userId, User updatedUser)
        {
            try
            {
                return (await usersAPI.UpdateUser(userId, updatedUser)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }
    }
}
