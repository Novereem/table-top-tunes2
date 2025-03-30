namespace Shared.Models.DTOs.Scenes
{
    public class SceneGetResponseDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}