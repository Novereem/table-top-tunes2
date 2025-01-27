﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class SceneAudioFile
    {
        public Guid SceneId { get; set; }
        public Guid AudioFileId { get; set; }
        public string AudioType { get; set; } = string.Empty;
        public Scene Scene { get; set; } = null!;
        public AudioFile AudioFile { get; set; } = null!;
    }
}
