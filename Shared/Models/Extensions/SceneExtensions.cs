using Shared.Models.DTOs.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static SceneListItemDTO ToSceneListItemDTO(this Scene scene)
        {
            return new SceneListItemDTO
            {
                Id = scene.Id,
                Name = scene.Name,
                CreatedAt = scene.CreatedAt
            };
        }
    }
}
