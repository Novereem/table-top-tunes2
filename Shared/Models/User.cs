using Shared.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class User : BaseEntity
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public List<Scene> Scenes { get; set; } = new List<Scene>();
        public List<AudioFile> AudioFiles { get; set; } = new List<AudioFile>();
    }
}
