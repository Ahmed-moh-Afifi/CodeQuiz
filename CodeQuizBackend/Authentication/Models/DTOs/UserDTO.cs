namespace CodeQuizBackend.Authentication.Models.DTOs
{
    public class UserDTO
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required DateTime JoinDate { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
