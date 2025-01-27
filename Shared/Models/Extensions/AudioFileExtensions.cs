using Shared.Models.DTOs.AudioFiles;

namespace Shared.Models.Extensions;

public static class AudioFileExtensions
{
    public static AudioFile ToAudioFromCreateDTO(this AudioFileCreateDTO dto)
    {
        return new AudioFile
        {
            Name = dto.Name,
            FilePath = "/path/to/file"
        };
    }

    public static AudioFileCreateResponseDTO ToCreateResponseDTO(this AudioFile audioFile)
    {
        return new AudioFileCreateResponseDTO
        {
            Id = audioFile.Id,
            Name = audioFile.Name,
            UserId = audioFile.UserId,
            CreatedAt = audioFile.CreatedAt
        };
    }
}