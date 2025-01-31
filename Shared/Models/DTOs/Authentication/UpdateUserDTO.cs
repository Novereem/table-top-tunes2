namespace Shared.Models.DTOs.Authentication;

public class UpdateUserDTO
{
    public string OldPassword { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    
    public string? NewPassword { get; set; }
    public long? UsedStorageBytes { get; set; }
    public long? MaxStorageBytes { get; set; }
}