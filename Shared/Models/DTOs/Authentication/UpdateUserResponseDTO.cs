namespace Shared.Models.DTOs.Authentication
{
    public class UpdateUserResponseDTO
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required long UsedStorageBytes { get; set; }
        public required long MaxStorageBytes  { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}