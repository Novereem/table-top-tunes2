namespace Shared.Models.Common;

public class DebugApiResponse<T>(T? data, string? userMessage, string? internalMessage)
    : ApiResponse<T>(data, userMessage)
{
    public string? InternalMessage { get; } = internalMessage;
}