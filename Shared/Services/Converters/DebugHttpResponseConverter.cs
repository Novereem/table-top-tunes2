using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Controllers;
using Shared.Models.Common;

namespace Shared.Services.Converters;

public class DebugHttpResponseConverter : IHttpResponseConverter
{
    public IActionResult Convert<T>(HttpServiceResult<T> serviceResult)
    {
        var apiResponse = new DebugApiResponse<T>(
            serviceResult.Data,
            serviceResult.MessageInfo.UserMessage,
            serviceResult.MessageInfo.InternalMessage
        );
        return new ObjectResult(apiResponse)
        {
            StatusCode = (int)serviceResult.HttpStatusCode
        };
    }
}