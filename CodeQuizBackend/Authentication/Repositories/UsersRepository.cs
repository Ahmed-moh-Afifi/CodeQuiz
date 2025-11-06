using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Models.DTOs;
using CodeQuizBackend.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeQuizBackend.Authentication.Repositories
{
    public class UsersRepository(ApplicationDbContext dbContext, UserManager<User> userManager) : IUsersRepository
    {
        public async Task<IEnumerable<UserDTO>> SearchUsers(string query, DateTime? lastDate, string? lastId)
        {
            try
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
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<UserDTO> GetUser(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    // throw new NotFoundException("User not found");
                }
                return user!.ToDTO();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateUser(UserDTO user)
        {
            try
            {
                var userModel = dbContext.Users.Find(user.Id);
                userModel?.UpdateFromDTO(user);
                if (userModel != null)
                {
                    dbContext.Users.Update(userModel);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> IsUserNameAvailable(string userName)
        {
            try
            {
                var user = await userManager.FindByNameAsync(userName);
                return user == null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
