namespace Shared.Models.DTOs.AudioStreaming
{
    public class AudioStreamResponseDTO
    {
        public Stream FileStream { get; init; } = null!;
        public string ContentType { get; init; } = null!;
        public long ContentLength { get; init; }
        public int StatusCode { get; init; }
        public AudioStreamHeadersDTO Headers { get; init; } = null!;
    }
}