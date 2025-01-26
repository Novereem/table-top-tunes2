using Microsoft.AspNetCore.Mvc;
using Shared.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extensions.Controllers
{
    public static class ControllerBaseExtensions
    {
        public static IActionResult ToActionResult<T>(this ControllerBase controller, HttpServiceResult<T> serviceResult)
        {
            var apiResponse = new ApiResponse<T>(serviceResult.Data, serviceResult.MessageInfo.UserMessage);
            return controller.StatusCode((int)serviceResult.HttpStatusCode, apiResponse);
        }
    }
}
