using Shared.Enums;

namespace Shared.Models
{
    public class SceneAudioFile
    {
        public Guid SceneId { get; set; }
        public Guid AudioFileId { get; set; }
        public AudioType AudioType { get; set; }
    }
}