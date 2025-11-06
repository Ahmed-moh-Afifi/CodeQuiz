using CodeQuizBackend.Authentication.Models.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CodeQuizBackend.Authentication.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public string? ProfilePicture { get; set; }

        public static User FromRegisterModel(RegisterModel model)
        {
            return new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Username,
            };
        }

        public UserDTO ToDTO()
        {
            return new UserDTO()
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName!,
                JoinDate = JoinDate,
                Email = Email!,
                ProfilePicture = ProfilePicture
            };
        }

        public void UpdateFromDTO(UserDTO dto)
        {
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            Email = dto.Email;
            UserName = dto.UserName;
        }
    }
}
