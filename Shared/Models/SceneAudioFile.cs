using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
