using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Controllers;
using Shared.Models.Common;

namespace Shared.Services.Converters;

public class ProductionHttpResponseConverter : IHttpResponseConverter
{
    public IActionResult Convert<T>(HttpServiceResult<T> serviceResult)
    {
        var apiResponse = new ApiResponse<T>(
            serviceResult.Data,
            serviceResult.MessageInfo.UserMessage
        );
        return new ObjectResult(apiResponse)
        {
            StatusCode = (int)serviceResult.HttpStatusCode
        };
    }
}