namespace Shared.Models.DTOs.Scenes
{
    public class SceneUpdateResponseDTO
    {
        public Guid SceneId { get; set; }
        public required string UpdatedName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}