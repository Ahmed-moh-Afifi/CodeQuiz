using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Models;

namespace CodeQuizDesktop.Repositories;

public class UsersRepository(IUsersAPI usersAPI) : IUsersRepository
{
    public async Task<User> GetUser()
    {
        try
        {
            return (await usersAPI.GetUser()).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to load user profile.");
        }
    }

    public async Task<bool> IsUsernameAvailable(string username)
    {
        try
        {
            return (await usersAPI.IsUsernameAvailable(username)).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to check username availability.");
        }
    }

    public async Task<List<User>> Search(string query)
    {
        try
        {
            return (await usersAPI.Search(query)).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to search users.");
        }
    }

    public async Task<User> UpdateUser(string userId, User updatedUser)
    {
        try
        {
            return (await usersAPI.UpdateUser(userId, updatedUser)).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to update user profile.");
        }
    }
}
