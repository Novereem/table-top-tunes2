using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DTOs.Scenes
{
    public class SceneCreateResponseDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
