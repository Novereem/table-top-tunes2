using Shared.Enums;

namespace Shared.Models.DTOs.SceneAudios
{
    public class SceneAudioRemoveDTO
    {
        public Guid SceneId { get; set; }
        public Guid AudioFileId { get; set; }
        public AudioType AudioType { get; set; }
    }
}