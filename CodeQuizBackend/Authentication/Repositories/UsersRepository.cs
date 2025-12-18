using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeQuizBackend.Authentication.Repositories
{
    public class UsersRepository(ApplicationDbContext dbContext, UserManager<User> userManager) : IUsersRepository
    {
        public async Task<IEnumerable<UserDTO>> SearchUsers(string query, DateTime? lastDate, string? lastId)
        {
            List<UserDTO> users;
            if (lastDate == null && lastId == null)
            {
                users = await dbContext.Users
                    .Where(user =>
                        user.UserName!.Contains(query) ||
                        user.FirstName.Contains(query) ||
                        user.LastName.Contains(query) ||
                        query.Contains(user.UserName) ||
                        query.Contains(user.FirstName) ||
                        query.Contains(user.LastName))
                    .OrderBy(u => u.JoinDate)
                    .ThenBy(u => u.Id)
                    .Select(u => u.ToDTO())
                    .Take(10)
                    .ToListAsync();
            }
            else
            {
                users = await dbContext.Users
                    .Where(user =>
                        user.UserName!.Contains(query) ||
                        user.FirstName.Contains(query) ||
                        user.LastName.Contains(query) ||
                        query.Contains(user.UserName) ||
                        query.Contains(user.FirstName) ||
                        query.Contains(user.LastName))
                    .OrderBy(u => u.JoinDate)
                    .ThenBy(u => u.Id)
                    .Select(u => u.ToDTO())
                    .Where(u => u.JoinDate > lastDate || (u.JoinDate == lastDate && u.Id.CompareTo(lastId) > 0))
                    .Take(10)
                    .ToListAsync();
            }

            return users;
        }

        public async Task<UserDTO> GetUser(string id)
        {
            var user = await userManager.FindByIdAsync(id)
                ?? throw new ResourceNotFoundException("User not found.");
            return user.ToDTO();
        }

        public async Task UpdateUser(UserDTO user)
        {
            var userModel = await dbContext.Users.FindAsync(user.Id)
                ?? throw new ResourceNotFoundException("User not found.");
            
            userModel.UpdateFromDTO(user);
            dbContext.Users.Update(userModel);
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> IsUserNameAvailable(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            return user == null;
        }
    }
}
