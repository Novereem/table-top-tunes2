namespace Shared.Models.DTOs.Authentication
{
    public class RegisterDTO
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
