namespace Shared.Models.DTOs.AudioFiles;

public class AudioFileCreateResponseDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}