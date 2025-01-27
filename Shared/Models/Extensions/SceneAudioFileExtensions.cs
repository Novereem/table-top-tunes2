using Shared.Models.DTOs.SceneAudios;

namespace Shared.Models.Extensions
{
    public static class SceneAudioFileExtensions
    {
        public static SceneAudioFile ToSceneAudioFileFromAssignDTO(this SceneAudioAssignDTO sceneAudioAssignDTO)
        {
            return new SceneAudioFile
            {
                SceneId = sceneAudioAssignDTO.SceneId,
                AudioFileId = sceneAudioAssignDTO.AudioFileId,
                AudioType = sceneAudioAssignDTO.AudioType
            };
        }

        public static SceneAudioAssignResponseDTO ToSceneAudioAssignDTO(this SceneAudioFile sceneAudioFile)
        {
            return new SceneAudioAssignResponseDTO
            {
                SceneId = sceneAudioFile.SceneId,
                AudioFileId = sceneAudioFile.AudioFileId,
                AudioType = sceneAudioFile.AudioType
            };
        }
        
        public static SceneAudioFile ToSceneAudioFileFromRemoveDTO(this SceneAudioRemoveDTO sceneAudioRemoveDTO)
        {
            return new SceneAudioFile
            {
                SceneId = sceneAudioRemoveDTO.SceneId,
                AudioFileId = sceneAudioRemoveDTO.AudioFileId,
                AudioType = sceneAudioRemoveDTO.AudioType
            };
        }
    }
}