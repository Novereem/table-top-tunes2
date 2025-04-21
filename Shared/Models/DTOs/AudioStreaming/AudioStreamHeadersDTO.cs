namespace Shared.Models.DTOs.AudioStreaming
{
    public class AudioStreamHeadersDTO
    {
        public string AcceptRanges { get; set; } = "bytes";
        public string? ContentRange { get; set; }
    }
}