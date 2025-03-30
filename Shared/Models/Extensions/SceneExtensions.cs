using Shared.Models.DTOs.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.DTOs.SceneAudios;

namespace Shared.Models.Extensions
{
    public static class SceneExtensions
    {
        public static Scene ToSceneFromCreateDTO(this SceneCreateDTO dto)
        {
            return new Scene
            {
                Name = dto.Name
            };
        }
        public static SceneCreateResponseDTO ToCreateResponseDTO(this Scene scene)
        {
            return new SceneCreateResponseDTO
            {
                Id = scene.Id,
                Name = scene.Name,
                CreatedAt = scene.CreatedAt
            };
        }
        public static SceneListItemDTO ToListItemDTO(this Scene scene)
        {
            return new SceneListItemDTO
            {
                Id = scene.Id,
                Name = scene.Name,
                CreatedAt = scene.CreatedAt
            };
        }

        public static SceneAudioRemoveAllDTO ToSceneAudioRemoveAllDtoFromRemoveDTO(this SceneRemoveDTO sceneAudioRemoveDTO)
        {
            return new SceneAudioRemoveAllDTO
            {
                SceneId = sceneAudioRemoveDTO.SceneId,
            };
        }

        public static SceneUpdateResponseDTO ToUpdateResponseDTO(this Scene scene)
        {
            return new SceneUpdateResponseDTO
            {
                SceneId = scene.Id,
                UpdatedName = scene.Name,
                CreatedAt = scene.CreatedAt
            };
        }

        public static SceneGetResponseDTO ToGetResponseDTO(this Scene scene)
        {
            return new SceneGetResponseDTO
            {
                Id = scene.Id,
                Name = scene.Name,
                UserId = scene.UserId,
                CreatedAt = scene.CreatedAt
            };
        }

        public static Scene ToSceneFromGetResponseDTO(this SceneGetResponseDTO sceneGetResponseDTO)
        {
            return new Scene
            {
                Id = sceneGetResponseDTO.Id,
                Name = sceneGetResponseDTO.Name,
                UserId = sceneGetResponseDTO.UserId,
                CreatedAt = sceneGetResponseDTO.CreatedAt
            };
        }
    }
}
