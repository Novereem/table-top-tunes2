using Microsoft.AspNetCore.Http;

namespace Shared.Models.DTOs.AudioFiles
{
    public class AudioFileCreateDTO
    {
        public IFormFile AudioFile { get; set; }
        public required string Name { get; set; }
    }
}