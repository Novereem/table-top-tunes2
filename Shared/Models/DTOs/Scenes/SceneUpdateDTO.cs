namespace Shared.Models.DTOs.Scenes
{
    public class SceneUpdateDTO
    {
        public Guid SceneId { get; set; }
        public required string NewName { get; set; }
    }
}