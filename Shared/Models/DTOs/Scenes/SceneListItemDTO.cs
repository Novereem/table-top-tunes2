namespace Shared.Models.DTOs.Scenes
{
    public class SceneListItemDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
