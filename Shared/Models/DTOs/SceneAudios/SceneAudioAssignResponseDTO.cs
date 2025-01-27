using Shared.Enums;

namespace Shared.Models.DTOs.SceneAudios
{
    public class SceneAudioAssignResponseDTO
    {
        public Guid SceneId { get; set; }
        public Guid AudioFileId { get; set; }
        public AudioType AudioType { get; set; }
    }
}